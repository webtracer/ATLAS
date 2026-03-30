using Amdocs.Atlas.Core.DTOs;

namespace Amdocs.Atlas.Api.Services;

/// <summary>
/// Service for connecting to vSphere and collecting infrastructure data
/// </summary>
public interface IVSphereClientService
{
    /// <summary>
    /// Test connection to vSphere with provided credentials
    /// </summary>
    Task<bool> TestConnectionAsync(VSphereConnectionInfo connectionInfo, string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of clusters from vSphere
    /// </summary>
    Task<List<string>> GetClustersAsync(VSphereConnectionInfo connectionInfo, string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Collect performance metrics for a cluster
    /// </summary>
    Task<Dictionary<string, object>> GetClusterPerformanceMetricsAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        string clusterName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Collect capacity data for a cluster
    /// </summary>
    Task<Dictionary<string, object>> GetClusterCapacityDataAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        string clusterName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Collect general infrastructure data
    /// </summary>
    Task<Dictionary<string, object>> GetInfrastructureDataAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        string? clusterName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get VM inventory from a vCenter.
    /// </summary>
    Task<List<VSphereVmInventoryItem>> GetVmInventoryAsync(
        VSphereConnectionInfo connectionInfo,
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
