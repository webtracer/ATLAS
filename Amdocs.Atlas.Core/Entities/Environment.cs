namespace Amdocs.Atlas.Core.Entities;

public class Environment
{
    public int EnvironmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Server> Servers { get; set; } = new List<Server>();
}