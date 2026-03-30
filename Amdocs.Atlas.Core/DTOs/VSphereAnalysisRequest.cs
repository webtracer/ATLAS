namespace Amdocs.Atlas.Core.DTOs;

/// <summary>
/// Request DTO for vSphere analysis operations
/// Credentials are provided per-request and not stored
/// </summary>
public class VSphereAnalysisRequest
{
    public int VcenterId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ClusterName { get; set; }
    public string? AnalysisType { get; set; }  // "performance", "capacity", "logs", "general"
    public Dictionary<string, object>? AdditionalParameters { get; set; }
}
