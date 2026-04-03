using System.Text.Json;
using Amdocs.Atlas.Core.DTOs;
using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.EntityFrameworkCore;

namespace Amdocs.Atlas.Api.Services;

public class VSphereInventorySyncOptions
{
    public bool Enabled { get; set; } = false;
    public bool RunOnStartup { get; set; } = false;
    public int RunHourLocal { get; set; } = 2;
    public int RunMinuteLocal { get; set; } = 0;
    public int DefaultEnvironmentId { get; set; } = 1;
    public int DefaultRoleId { get; set; } = 1;
}

public class VSphereInventorySyncService : BackgroundService
{
    private readonly ILogger<VSphereInventorySyncService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public VSphereInventorySyncService(
        ILogger<VSphereInventorySyncService> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = ReadOptions();
        if (!options.Enabled)
        {
            _logger.LogInformation("vSphere daily inventory sync is disabled.");
            return;
        }

        if (options.RunOnStartup)
        {
            await RunSyncAsync(options, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, options.RunHourLocal, options.RunMinuteLocal, 0);
            if (nextRun <= now)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;
            _logger.LogInformation("Next vSphere inventory sync scheduled at {NextRunLocal}", nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await RunSyncAsync(options, stoppingToken);
        }
    }

    private async Task RunSyncAsync(VSphereInventorySyncOptions options, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
        var client = scope.ServiceProvider.GetRequiredService<IVSphereClientService>();

        var started = DateTime.UtcNow;
        var totalCreated = 0;
        var totalUpdated = 0;
        var totalDeactivated = 0;
        var totalSkipped = 0;

        _logger.LogInformation("Starting vSphere inventory sync.");

        var defaultEnvironmentExists = await db.Environments
            .AsNoTracking()
            .AnyAsync(e => e.EnvironmentId == options.DefaultEnvironmentId, cancellationToken);
        var defaultRoleExists = await db.Roles
            .AsNoTracking()
            .AnyAsync(r => r.RoleId == options.DefaultRoleId, cancellationToken);

        if (!defaultEnvironmentExists || !defaultRoleExists)
        {
            _logger.LogError(
                "vSphere sync defaults invalid. EnvironmentId {EnvironmentId} exists: {EnvironmentExists}, RoleId {RoleId} exists: {RoleExists}",
                options.DefaultEnvironmentId,
                defaultEnvironmentExists,
                options.DefaultRoleId,
                defaultRoleExists);
            return;
        }

        var vcenters = await db.Vcenters
            .Where(v => v.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var vcenter in vcenters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryExtractCredentials(vcenter.Passphrase, out var username, out var password))
            {
                totalSkipped++;
                _logger.LogWarning(
                    "Skipping vCenter {VcenterId} ({VcenterName}). Passphrase is missing or not parseable. Expected JSON {{\"username\":\"...\",\"password\":\"...\"}} or username|password.",
                    vcenter.VcenterId,
                    vcenter.Name ?? vcenter.IpAddress);
                continue;
            }

            var connectionInfo = new VSphereConnectionInfo
            {
                VcenterId = vcenter.VcenterId,
                Name = vcenter.Name ?? $"vCenter-{vcenter.VcenterId}",
                Host = string.IsNullOrWhiteSpace(vcenter.Host) ? vcenter.IpAddress : vcenter.Host!,
                Port = vcenter.Port,
                SslVerify = vcenter.SslVerify,
                Description = vcenter.Description
            };

            try
            {
                var inventory = await client.GetVmInventoryAsync(connectionInfo, username!, password!, cancellationToken);

                var activeVmServers = await db.Servers
                    .Where(s => s.IsVm && s.VcenterId == vcenter.VcenterId && s.SourceType == "VMware")
                    .ToListAsync(cancellationToken);

                var seenHostnames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var vm in inventory)
                {
                    var normalizedName = vm.Name.Trim();
                    if (string.IsNullOrWhiteSpace(normalizedName))
                    {
                        continue;
                    }

                    seenHostnames.Add(normalizedName);

                    var existing = FindExisting(activeVmServers, vm, vcenter.VcenterId);
                    if (existing is null)
                    {
                        db.Servers.Add(new Server
                        {
                            Hostname = normalizedName,
                            Fqdn = normalizedName,
                            IpAddress = vm.PrimaryIpAddress,
                            IsVm = true,
                            SourceType = "VMware",
                            VmInstanceUuid = vm.InstanceUuid,
                            VmBiosUuid = vm.BiosUuid,
                            OsName = vm.GuestOs,
                            LifecycleStatus = "In Service",
                            IsActive = true,
                            EnvironmentId = options.DefaultEnvironmentId,
                            RoleId = options.DefaultRoleId,
                            VcenterId = vcenter.VcenterId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        totalCreated++;
                    }
                    else
                    {
                        existing.Hostname = normalizedName;
                        existing.Fqdn = string.IsNullOrWhiteSpace(existing.Fqdn) ? normalizedName : existing.Fqdn;
                        existing.IpAddress = vm.PrimaryIpAddress ?? existing.IpAddress;
                        existing.OsName = vm.GuestOs ?? existing.OsName;
                        existing.VmInstanceUuid = vm.InstanceUuid ?? existing.VmInstanceUuid;
                        existing.VmBiosUuid = vm.BiosUuid ?? existing.VmBiosUuid;
                        existing.IsVm = true;
                        existing.SourceType = "VMware";
                        existing.VcenterId = vcenter.VcenterId;
                        existing.IsActive = true;
                        existing.LifecycleStatus = "In Service";
                        existing.DecommissionedAt = null;
                        existing.UpdatedAt = DateTime.UtcNow;
                        totalUpdated++;
                    }
                }

                foreach (var server in activeVmServers.Where(s =>
                             s.IsActive &&
                             !string.IsNullOrWhiteSpace(s.Hostname) &&
                             !seenHostnames.Contains(s.Hostname)))
                {
                    server.IsActive = false;
                    server.LifecycleStatus = "Not seen in vSphere sync";
                    server.DecommissionedAt = DateTime.UtcNow;
                    server.UpdatedAt = DateTime.UtcNow;
                    totalDeactivated++;
                }

                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                totalSkipped++;
                _logger.LogError(
                    ex,
                    "Failed inventory sync for vCenter {VcenterId} ({VcenterName})",
                    vcenter.VcenterId,
                    vcenter.Name ?? vcenter.IpAddress);
            }
        }

        var elapsedSeconds = (DateTime.UtcNow - started).TotalSeconds;
        _logger.LogInformation(
            "vSphere inventory sync finished in {ElapsedSeconds:F1}s. Created: {Created}, Updated: {Updated}, Deactivated: {Deactivated}, SkippedVcenters: {Skipped}",
            elapsedSeconds,
            totalCreated,
            totalUpdated,
            totalDeactivated,
            totalSkipped);
    }

    private static Server? FindExisting(List<Server> servers, VSphereVmInventoryItem vm, int vcenterId)
    {
        if (!string.IsNullOrWhiteSpace(vm.InstanceUuid))
        {
            var byInstanceUuid = servers.FirstOrDefault(s =>
                !string.IsNullOrWhiteSpace(s.VmInstanceUuid) &&
                s.VcenterId == vcenterId &&
                string.Equals(s.VmInstanceUuid, vm.InstanceUuid, StringComparison.OrdinalIgnoreCase));
            if (byInstanceUuid is not null)
            {
                return byInstanceUuid;
            }
        }

        if (!string.IsNullOrWhiteSpace(vm.BiosUuid))
        {
            var byBiosUuid = servers.FirstOrDefault(s =>
                !string.IsNullOrWhiteSpace(s.VmBiosUuid) &&
                s.VcenterId == vcenterId &&
                string.Equals(s.VmBiosUuid, vm.BiosUuid, StringComparison.OrdinalIgnoreCase));
            if (byBiosUuid is not null)
            {
                return byBiosUuid;
            }
        }

        return servers.FirstOrDefault(s =>
            s.VcenterId == vcenterId &&
            !string.IsNullOrWhiteSpace(s.Hostname) &&
            string.Equals(s.Hostname, vm.Name, StringComparison.OrdinalIgnoreCase));
    }

    private VSphereInventorySyncOptions ReadOptions()
    {
        var options = new VSphereInventorySyncOptions();
        _configuration.GetSection("VSphereSync").Bind(options);
        return options;
    }

    private static bool TryExtractCredentials(string? passphrase, out string? username, out string? password)
    {
        username = null;
        password = null;

        if (string.IsNullOrWhiteSpace(passphrase))
        {
            return false;
        }

        var trimmed = passphrase.Trim();

        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
        {
            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(trimmed);
                if (json.TryGetProperty("username", out var usernameProp) &&
                    json.TryGetProperty("password", out var passwordProp))
                {
                    username = usernameProp.GetString();
                    password = passwordProp.GetString();
                    return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
                }
            }
            catch
            {
                return false;
            }
        }

        var pipeParts = trimmed.Split('|', 2, StringSplitOptions.TrimEntries);
        if (pipeParts.Length == 2 &&
            !string.IsNullOrWhiteSpace(pipeParts[0]) &&
            !string.IsNullOrWhiteSpace(pipeParts[1]))
        {
            username = pipeParts[0];
            password = pipeParts[1];
            return true;
        }

        var colonParts = trimmed.Split(':', 2, StringSplitOptions.TrimEntries);
        if (colonParts.Length == 2 &&
            !string.IsNullOrWhiteSpace(colonParts[0]) &&
            !string.IsNullOrWhiteSpace(colonParts[1]))
        {
            username = colonParts[0];
            password = colonParts[1];
            return true;
        }

        return false;
    }
}
