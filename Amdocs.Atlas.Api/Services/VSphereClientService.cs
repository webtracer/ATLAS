using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Amdocs.Atlas.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace Amdocs.Atlas.Api.Services;

/// <summary>
/// Implementation of vSphere client service using vSphere Automation REST API
/// Uses REST API instead of legacy SDK - better for .NET
/// </summary>
public class VSphereClientService : IVSphereClientService
{
    private readonly ILogger<VSphereClientService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public VSphereClientService(
        ILogger<VSphereClientService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }
    
    private string GetBaseUrl(VSphereConnectionInfo connectionInfo)
    {
        var protocol = connectionInfo.SslVerify ? "https" : "https"; // Always HTTPS for vSphere
        return $"{protocol}://{connectionInfo.Host}:{connectionInfo.Port}";
    }

    private HttpClient CreateHttpClient(VSphereConnectionInfo connectionInfo)
    {
        var handler = new HttpClientHandler();
        if (!connectionInfo.SslVerify)
        {
            handler.ServerCertificateCustomValidationCallback = 
                (message, cert, chain, errors) => true;
        }
        return new HttpClient(handler);
    }

    public async Task<bool> TestConnectionAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        HttpClient? client = null;
        try
        {
            _logger.LogInformation("Testing connection to vSphere {Host}:{Port}", connectionInfo.Host, connectionInfo.Port);
            
            var sessionToken = await AuthenticateAsync(connectionInfo, username, password, cancellationToken);
            
            if (string.IsNullOrEmpty(sessionToken))
            {
                return false;
            }

            var baseUrl = GetBaseUrl(connectionInfo);
            client = CreateHttpClient(connectionInfo);
            client.DefaultRequestHeaders.Add("vmware-api-session-id", sessionToken);
            
            var response = await client.GetAsync($"{baseUrl}/rest/vcenter", cancellationToken);
            
            await LogoutAsync(connectionInfo, sessionToken, cancellationToken);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to vSphere {Host}", connectionInfo.Host);
            return false;
        }
        finally
        {
            client?.Dispose();
        }
    }

    private async Task<string> AuthenticateAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        HttpClient? client = null;
        try
        {
            var baseUrl = GetBaseUrl(connectionInfo);
            client = CreateHttpClient(connectionInfo);
            
            // vSphere REST API session endpoint uses Basic Auth ONLY (no body)
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/rest/com/vmware/cis/session");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

            _logger.LogInformation("Authenticating to vSphere {Host}:{Port} as {Username}", 
                connectionInfo.Host, connectionInfo.Port, username);

            var response = await client.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var sessionToken = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (sessionToken.TryGetProperty("value", out var value))
                    {
                        var token = value.GetString() ?? string.Empty;
                        _logger.LogInformation("Successfully authenticated to vSphere {Host}", connectionInfo.Host);
                        return token;
                    }
                    else
                    {
                        _logger.LogWarning("Authentication response missing 'value' property. Response: {Response}", responseContent);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse authentication response. Response: {Response}", responseContent);
                }
            }
            else
            {
                _logger.LogError(
                    "Authentication failed for vSphere {Host}:{Port}. Status: {Status}, Response: {Response}",
                    connectionInfo.Host, connectionInfo.Port, response.StatusCode, responseContent);
            }

            return string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during authentication to vSphere {Host}:{Port}. Message: {Message}", 
                connectionInfo.Host, connectionInfo.Port, ex.Message);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication to vSphere {Host}:{Port}", 
                connectionInfo.Host, connectionInfo.Port);
            return string.Empty;
        }
        finally
        {
            client?.Dispose();
        }
    }

    private async Task LogoutAsync(
        VSphereConnectionInfo connectionInfo,
        string sessionToken,
        CancellationToken cancellationToken)
    {
        HttpClient? client = null;
        try
        {
            var baseUrl = GetBaseUrl(connectionInfo);
            client = CreateHttpClient(connectionInfo);
            client.DefaultRequestHeaders.Add("vmware-api-session-id", sessionToken);
            
            await client.DeleteAsync($"{baseUrl}/rest/com/vmware/cis/session", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to logout from vSphere {Host}", connectionInfo.Host);
        }
        finally
        {
            client?.Dispose();
        }
    }

    public async Task<List<string>> GetClustersAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving clusters from vSphere {Host}", connectionInfo.Host);
            
            var sessionToken = await AuthenticateAsync(connectionInfo, username, password, cancellationToken);
            if (string.IsNullOrEmpty(sessionToken))
            {
                throw new InvalidOperationException("Failed to authenticate to vSphere");
            }

            HttpClient? client = null;
            try
            {
                var baseUrl = GetBaseUrl(connectionInfo);
                client = CreateHttpClient(connectionInfo);
                client.DefaultRequestHeaders.Add("vmware-api-session-id", sessionToken);
                
                var response = await client.GetAsync($"{baseUrl}/rest/vcenter/cluster", cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var clustersJson = JsonSerializer.Deserialize<JsonElement>(content);
                
                var clusters = new List<string>();
                if (clustersJson.TryGetProperty("value", out var valueArray))
                {
                    foreach (var cluster in valueArray.EnumerateArray())
                    {
                        if (cluster.TryGetProperty("name", out var name))
                        {
                            clusters.Add(name.GetString() ?? string.Empty);
                        }
                    }
                }
                
                return clusters;
            }
            finally
            {
                client?.Dispose();
                await LogoutAsync(connectionInfo, sessionToken, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve clusters from vSphere {Host}", connectionInfo.Host);
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetClusterPerformanceMetricsAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        string clusterName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Collecting performance metrics for cluster {Cluster} from {Host}",
                clusterName, connectionInfo.Host);

            var sessionToken = await AuthenticateAsync(connectionInfo, username, password, cancellationToken);
            if (string.IsNullOrEmpty(sessionToken))
            {
                throw new InvalidOperationException("Failed to authenticate to vSphere");
            }

            HttpClient? client = null;
            try
            {
                var baseUrl = GetBaseUrl(connectionInfo);
                client = CreateHttpClient(connectionInfo);
                client.DefaultRequestHeaders.Add("vmware-api-session-id", sessionToken);
                
                var metrics = new Dictionary<string, object>
                {
                    ["cluster_name"] = clusterName,
                    ["timestamp"] = DateTime.UtcNow
                };

                var hostsResponse = await client.GetAsync($"{baseUrl}/rest/vcenter/host", cancellationToken);
                if (hostsResponse.IsSuccessStatusCode)
                {
                    var hostsContent = await hostsResponse.Content.ReadAsStringAsync(cancellationToken);
                    var hostsJson = JsonSerializer.Deserialize<JsonElement>(hostsContent);
                    var hostsList = new List<Dictionary<string, object>>();
                    
                    if (hostsJson.TryGetProperty("value", out var hostsArray))
                    {
                        foreach (var host in hostsArray.EnumerateArray())
                        {
                            if (host.TryGetProperty("host", out var hostId))
                            {
                                var hostIdStr = hostId.GetString();
                                if (!string.IsNullOrEmpty(hostIdStr))
                                {
                                    var hostMetrics = await GetHostMetricsAsync(client, baseUrl, hostIdStr, cancellationToken);
                                    if (hostMetrics != null)
                                    {
                                        hostsList.Add(hostMetrics);
                                    }
                                }
                            }
                        }
                    }
                    metrics["hosts"] = hostsList;
                }

                var vmsResponse = await client.GetAsync($"{baseUrl}/rest/vcenter/vm", cancellationToken);
                if (vmsResponse.IsSuccessStatusCode)
                {
                    var vmsContent = await vmsResponse.Content.ReadAsStringAsync(cancellationToken);
                    var vmsJson = JsonSerializer.Deserialize<JsonElement>(vmsContent);
                    var vmsList = new List<Dictionary<string, object>>();
                    
                    if (vmsJson.TryGetProperty("value", out var vmsArray))
                    {
                        foreach (var vm in vmsArray.EnumerateArray())
                        {
                            var vmData = new Dictionary<string, object>();
                            if (vm.TryGetProperty("name", out var name))
                                vmData["name"] = name.GetString() ?? "";
                            if (vm.TryGetProperty("power_state", out var powerState))
                                vmData["power_state"] = powerState.GetString() ?? "";
                            if (vm.TryGetProperty("cpu", out var cpu) && cpu.TryGetProperty("count", out var cpuCount))
                                vmData["cpu_count"] = cpuCount.GetInt32();
                            if (vm.TryGetProperty("memory", out var memory) && memory.TryGetProperty("size_MiB", out var memorySize))
                                vmData["memory_mb"] = memorySize.GetInt64();
                            
                            vmsList.Add(vmData);
                        }
                    }
                    metrics["vms"] = vmsList;
                }

                var datastoresResponse = await client.GetAsync($"{baseUrl}/rest/vcenter/datastore", cancellationToken);
                if (datastoresResponse.IsSuccessStatusCode)
                {
                    var dsContent = await datastoresResponse.Content.ReadAsStringAsync(cancellationToken);
                    var dsJson = JsonSerializer.Deserialize<JsonElement>(dsContent);
                    var dsList = new List<Dictionary<string, object>>();
                    
                    if (dsJson.TryGetProperty("value", out var dsArray))
                    {
                        foreach (var ds in dsArray.EnumerateArray())
                        {
                            var dsData = new Dictionary<string, object>();
                            if (ds.TryGetProperty("name", out var name))
                                dsData["name"] = name.GetString() ?? "";
                            if (ds.TryGetProperty("capacity", out var capacity))
                                dsData["capacity_gb"] = capacity.GetInt64() / (1024 * 1024 * 1024);
                            if (ds.TryGetProperty("free_space", out var freeSpace))
                                dsData["free_space_gb"] = freeSpace.GetInt64() / (1024 * 1024 * 1024);
                            
                            dsList.Add(dsData);
                        }
                    }
                    metrics["datastores"] = dsList;
                }

                return metrics;
            }
            finally
            {
                client?.Dispose();
                await LogoutAsync(connectionInfo, sessionToken, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect performance metrics for cluster {Cluster}", clusterName);
            throw;
        }
    }

    private async Task<Dictionary<string, object>?> GetHostMetricsAsync(
        HttpClient client,
        string baseUrl,
        string hostId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetAsync($"{baseUrl}/rest/vcenter/host/{hostId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var hostJson = JsonSerializer.Deserialize<JsonElement>(content);
            
            var metrics = new Dictionary<string, object>();
            if (hostJson.TryGetProperty("value", out var value))
            {
                if (value.TryGetProperty("name", out var name))
                    metrics["name"] = name.GetString() ?? "";
                if (value.TryGetProperty("connection_state", out var connState))
                    metrics["connection_state"] = connState.GetString() ?? "";
                if (value.TryGetProperty("power_state", out var powerState))
                    metrics["power_state"] = powerState.GetString() ?? "";
            }
            
            return metrics;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Dictionary<string, object>> GetClusterCapacityDataAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        string clusterName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Collecting capacity data for cluster {Cluster} from {Host}",
                clusterName, connectionInfo.Host);

            var performanceMetrics = await GetClusterPerformanceMetricsAsync(
                connectionInfo, username, password, clusterName, cancellationToken);

            var capacityData = new Dictionary<string, object>
            {
                ["cluster_name"] = clusterName,
                ["timestamp"] = DateTime.UtcNow
            };

            if (performanceMetrics.TryGetValue("hosts", out var hostsObj) && hostsObj is List<Dictionary<string, object>> hosts)
            {
                capacityData["num_hosts"] = hosts.Count;
            }

            if (performanceMetrics.TryGetValue("vms", out var vmsObj) && vmsObj is List<Dictionary<string, object>> vms)
            {
                capacityData["num_vms"] = vms.Count;
            }

            if (performanceMetrics.TryGetValue("datastores", out var dsObj) && dsObj is List<Dictionary<string, object>> datastores)
            {
                capacityData["num_datastores"] = datastores.Count;
                
                long totalCapacity = 0;
                long totalFree = 0;
                foreach (var ds in datastores)
                {
                    if (ds.TryGetValue("capacity_gb", out var cap) && cap is long capLong)
                        totalCapacity += capLong;
                    if (ds.TryGetValue("free_space_gb", out var free) && free is long freeLong)
                        totalFree += freeLong;
                }
                
                capacityData["total_datastore_capacity_gb"] = totalCapacity;
                capacityData["total_datastore_free_gb"] = totalFree;
                capacityData["total_datastore_used_gb"] = totalCapacity - totalFree;
                if (totalCapacity > 0)
                {
                    capacityData["average_datastore_usage_percent"] = 
                        Math.Round((double)(totalCapacity - totalFree) / totalCapacity * 100, 2);
                }
            }

            return capacityData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect capacity data for cluster {Cluster}", clusterName);
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetInfrastructureDataAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        string? clusterName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Collecting infrastructure data from {Host}", connectionInfo.Host);

            var data = new Dictionary<string, object>
            {
                ["vcenter"] = connectionInfo.Name,
                ["timestamp"] = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(clusterName))
            {
                var performanceMetrics = await GetClusterPerformanceMetricsAsync(
                    connectionInfo, username, password, clusterName, cancellationToken);
                var capacityData = await GetClusterCapacityDataAsync(
                    connectionInfo, username, password, clusterName, cancellationToken);

                data["cluster"] = clusterName;
                data["performance"] = performanceMetrics;
                data["capacity"] = capacityData;
            }
            else
            {
                var clusters = await GetClustersAsync(connectionInfo, username, password, cancellationToken);
                data["clusters"] = clusters;
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect infrastructure data from {Host}", connectionInfo.Host);
            throw;
        }
    }

    public async Task<List<VSphereVmInventoryItem>> GetVmInventoryAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var inventory = new List<VSphereVmInventoryItem>();

        try
        {
            _logger.LogInformation("Collecting VM inventory from {Host}", connectionInfo.Host);

            var sessionToken = await AuthenticateAsync(connectionInfo, username, password, cancellationToken);
            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                throw new InvalidOperationException("Failed to authenticate to vSphere.");
            }

            HttpClient? client = null;
            try
            {
                var baseUrl = GetBaseUrl(connectionInfo);
                client = CreateHttpClient(connectionInfo);
                client.DefaultRequestHeaders.Add("vmware-api-session-id", sessionToken);

                var response = await client.GetAsync($"{baseUrl}/rest/vcenter/vm", cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var root = JsonSerializer.Deserialize<JsonElement>(content);

                if (!root.TryGetProperty("value", out var items))
                {
                    return inventory;
                }

                foreach (var vm in items.EnumerateArray())
                {
                    var item = new VSphereVmInventoryItem
                    {
                        Name = TryGetString(vm, "name") ?? string.Empty,
                        InstanceUuid = TryGetString(vm, "instance_uuid"),
                        BiosUuid = TryGetString(vm, "bios_uuid"),
                        GuestOs = TryGetString(vm, "guest_OS"),
                        PowerState = TryGetString(vm, "power_state"),
                        PrimaryIpAddress = TryGetString(vm, "ip_address")
                    };

                    if (vm.TryGetProperty("cpu_count", out var cpuCount) && cpuCount.ValueKind == JsonValueKind.Number)
                    {
                        item.CpuCount = cpuCount.GetInt32();
                    }

                    if (vm.TryGetProperty("memory_size_MiB", out var memory) && memory.ValueKind == JsonValueKind.Number)
                    {
                        item.MemoryMiB = memory.GetInt64();
                    }

                    if (!string.IsNullOrWhiteSpace(item.Name))
                    {
                        inventory.Add(item);
                    }
                }

                return inventory;
            }
            finally
            {
                client?.Dispose();
                await LogoutAsync(connectionInfo, sessionToken, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect VM inventory from {Host}", connectionInfo.Host);
            throw;
        }
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }
}
