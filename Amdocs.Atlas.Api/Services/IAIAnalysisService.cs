using Amdocs.Atlas.Core.DTOs;

namespace Amdocs.Atlas.Api.Services;

/// <summary>
/// Service for AI-powered analysis using CoPilot
/// </summary>
public interface IAIAnalysisService
{
    /// <summary>
    /// Analyze vSphere infrastructure data using AI
    /// </summary>
    Task<VSphereAnalysisResult> AnalyzeInfrastructureAsync(
        Dictionary<string, object> infrastructureData,
        string analysisType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate AI recommendations based on analysis
    /// </summary>
    Task<List<string>> GenerateRecommendationsAsync(
        Dictionary<string, object> metrics,
        string analysisType,
        CancellationToken cancellationToken = default);
}
