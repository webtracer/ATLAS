using Amdocs.Atlas.Core.DTOs;
using Amdocs.Atlas.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Amdocs.Atlas.Api.Services;

/// <summary>
/// Main service for vSphere AI analysis operations
/// Coordinates between vSphere client and AI analysis services
/// </summary>
public interface IVSphereAnalysisService
{
    Task<VSphereAnalysisResult> AnalyzeAsync(VSphereAnalysisRequest request, CancellationToken cancellationToken = default);
    Task<List<string>> GetClustersAsync(int vcenterId, string username, string password, CancellationToken cancellationToken = default);
}

public class VSphereAnalysisService : IVSphereAnalysisService
{
    private readonly AtlasDbContext _dbContext;
    private readonly IVSphereClientService _vsphereClient;
    private readonly IAIAnalysisService _aiAnalysis;
    private readonly ILogger<VSphereAnalysisService> _logger;

    public VSphereAnalysisService(
        AtlasDbContext dbContext,
        IVSphereClientService vsphereClient,
        IAIAnalysisService aiAnalysis,
        ILogger<VSphereAnalysisService> logger)
    {
        _dbContext = dbContext;
        _vsphereClient = vsphereClient;
        _aiAnalysis = aiAnalysis;
        _logger = logger;
    }

    public async Task<VSphereAnalysisResult> AnalyzeAsync(
        VSphereAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var vcenter = await _dbContext.Vcenters
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VcenterId == request.VcenterId && v.IsActive, cancellationToken);

            if (vcenter == null)
            {
                return new VSphereAnalysisResult
                {
                    Success = false,
                    ErrorMessage = $"vCenter with ID {request.VcenterId} not found or inactive."
                };
            }

            var connectionInfo = new VSphereConnectionInfo
            {
                VcenterId = vcenter.VcenterId,
                Name = vcenter.Name ?? $"vCenter-{vcenter.VcenterId}",
                Host = string.IsNullOrEmpty(vcenter.Host) ? vcenter.IpAddress : vcenter.Host,
                Port = vcenter.Port,
                SslVerify = vcenter.SslVerify,
                Description = vcenter.Description
            };

            var analysisType = request.AnalysisType ?? "general";

            Dictionary<string, object> infrastructureData;
            
            if (!string.IsNullOrEmpty(request.ClusterName))
            {
                if (analysisType == "performance")
                {
                    infrastructureData = await _vsphereClient.GetClusterPerformanceMetricsAsync(
                        connectionInfo, request.Username, request.Password, request.ClusterName, cancellationToken);
                }
                else if (analysisType == "capacity")
                {
                    infrastructureData = await _vsphereClient.GetClusterCapacityDataAsync(
                        connectionInfo, request.Username, request.Password, request.ClusterName, cancellationToken);
                }
                else
                {
                    infrastructureData = await _vsphereClient.GetInfrastructureDataAsync(
                        connectionInfo, request.Username, request.Password, request.ClusterName, cancellationToken);
                }
            }
            else
            {
                infrastructureData = await _vsphereClient.GetInfrastructureDataAsync(
                    connectionInfo, request.Username, request.Password, null, cancellationToken);
            }

            var result = await _aiAnalysis.AnalyzeInfrastructureAsync(
                infrastructureData, analysisType, cancellationToken);

            result.VcenterName = connectionInfo.Name;
            result.ClusterName = request.ClusterName ?? "All Clusters";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform vSphere analysis for vCenter {VcenterId}", request.VcenterId);
            return new VSphereAnalysisResult
            {
                Success = false,
                ErrorMessage = $"Analysis failed: {ex.Message}",
                AnalysisTimestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<List<string>> GetClustersAsync(
        int vcenterId,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var vcenter = await _dbContext.Vcenters
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VcenterId == vcenterId && v.IsActive, cancellationToken);

            if (vcenter == null)
            {
                throw new InvalidOperationException($"vCenter with ID {vcenterId} not found or inactive.");
            }

            var connectionInfo = new VSphereConnectionInfo
            {
                VcenterId = vcenter.VcenterId,
                Name = vcenter.Name ?? $"vCenter-{vcenter.VcenterId}",
                Host = string.IsNullOrEmpty(vcenter.Host) ? vcenter.IpAddress : vcenter.Host,
                Port = vcenter.Port,
                SslVerify = vcenter.SslVerify
            };

            return await _vsphereClient.GetClustersAsync(
                connectionInfo, username, password, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get clusters for vCenter {VcenterId}", vcenterId);
            throw;
        }
    }
}
