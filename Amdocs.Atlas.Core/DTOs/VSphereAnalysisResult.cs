namespace Amdocs.Atlas.Core.DTOs;

/// <summary>
/// Result DTO for vSphere AI analysis
/// </summary>
public class VSphereAnalysisResult
{
    public bool Success { get; set; }
    public string VcenterName { get; set; } = string.Empty;
    public string ClusterName { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = string.Empty;
    public string AiAnalysis { get; set; } = string.Empty;  // CoPilot AI analysis
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}
