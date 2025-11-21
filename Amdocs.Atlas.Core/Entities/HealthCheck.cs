namespace Amdocs.Atlas.Core.Entities;

public class HealthCheck
{
    public int HealthCheckId { get; set; }
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;

    public DateTime CheckTime { get; set; } = DateTime.UtcNow;
    public bool Reachable { get; set; }
    public int? StatusCode { get; set; }
    public int? ResponseMs { get; set; }
    public string? Source { get; set; }  // e.g., "PingJob"
    public string? Notes { get; set; }
}