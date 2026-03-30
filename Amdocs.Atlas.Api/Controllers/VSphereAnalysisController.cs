using Amdocs.Atlas.Api.Services;
using Amdocs.Atlas.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Amdocs.Atlas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VSphereAnalysisController : ControllerBase
{
    private readonly IVSphereAnalysisService _analysisService;
    private readonly ILogger<VSphereAnalysisController> _logger;

    public VSphereAnalysisController(
        IVSphereAnalysisService analysisService,
        ILogger<VSphereAnalysisController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of clusters from a vCenter
    /// Requires credentials in request body (not stored)
    /// </summary>
    [HttpPost("vcenters/{vcenterId}/clusters")]
    public async Task<ActionResult<List<string>>> GetClusters(
        int vcenterId,
        [FromBody] VSphereCredentialsRequest credentials,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(credentials.Username) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                return BadRequest(new { error = "Username and password are required." });
            }

            var clusters = await _analysisService.GetClustersAsync(
                vcenterId, credentials.Username, credentials.Password, cancellationToken);

            return Ok(clusters);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clusters for vCenter {VcenterId}", vcenterId);
            return StatusCode(500, new { error = $"Failed to retrieve clusters: {ex.Message}" });
        }
    }

    /// <summary>
    /// Perform AI analysis on vSphere infrastructure
    /// Requires credentials in request body (not stored)
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<VSphereAnalysisResult>> Analyze(
        [FromBody] VSphereAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Username and password are required." });
            }

            if (request.VcenterId <= 0)
            {
                return BadRequest(new { error = "Valid vCenterId is required." });
            }

            var result = await _analysisService.AnalyzeAsync(request, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing vSphere analysis");
            return StatusCode(500, new VSphereAnalysisResult
            {
                Success = false,
                ErrorMessage = $"Analysis failed: {ex.Message}",
                AnalysisTimestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Perform performance analysis on a specific cluster
    /// </summary>
    [HttpPost("vcenters/{vcenterId}/clusters/{clusterName}/analyze/performance")]
    public async Task<ActionResult<VSphereAnalysisResult>> AnalyzePerformance(
        int vcenterId,
        string clusterName,
        [FromBody] VSphereCredentialsRequest credentials,
        CancellationToken cancellationToken = default)
    {
        var request = new VSphereAnalysisRequest
        {
            VcenterId = vcenterId,
            Username = credentials.Username,
            Password = credentials.Password,
            ClusterName = clusterName,
            AnalysisType = "performance"
        };

        return await Analyze(request, cancellationToken);
    }

    /// <summary>
    /// Perform capacity analysis on a specific cluster
    /// </summary>
    [HttpPost("vcenters/{vcenterId}/clusters/{clusterName}/analyze/capacity")]
    public async Task<ActionResult<VSphereAnalysisResult>> AnalyzeCapacity(
        int vcenterId,
        string clusterName,
        [FromBody] VSphereCredentialsRequest credentials,
        CancellationToken cancellationToken = default)
    {
        var request = new VSphereAnalysisRequest
        {
            VcenterId = vcenterId,
            Username = credentials.Username,
            Password = credentials.Password,
            ClusterName = clusterName,
            AnalysisType = "capacity"
        };

        return await Analyze(request, cancellationToken);
    }

    /// <summary>
    /// Perform log analysis on a specific cluster
    /// </summary>
    [HttpPost("vcenters/{vcenterId}/clusters/{clusterName}/analyze/logs")]
    public async Task<ActionResult<VSphereAnalysisResult>> AnalyzeLogs(
        int vcenterId,
        string clusterName,
        [FromBody] VSphereCredentialsRequest credentials,
        CancellationToken cancellationToken = default)
    {
        var request = new VSphereAnalysisRequest
        {
            VcenterId = vcenterId,
            Username = credentials.Username,
            Password = credentials.Password,
            ClusterName = clusterName,
            AnalysisType = "logs"
        };

        return await Analyze(request, cancellationToken);
    }
}

/// <summary>
/// DTO for vSphere credentials (used only in requests, never stored)
/// </summary>
public class VSphereCredentialsRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
