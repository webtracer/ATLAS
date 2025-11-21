namespace Amdocs.Atlas.Core.Entities;

public class Location
{
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "OnPrem-DC1"
    public string? Type { get; set; }                // "VMware", "Azure", "Physical"
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Server> Servers { get; set; } = new List<Server>();
}