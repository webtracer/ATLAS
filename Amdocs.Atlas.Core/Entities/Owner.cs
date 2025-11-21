namespace Amdocs.Atlas.Core.Entities;

public class Owner
{
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty; // team or individual
    public string? Email { get; set; }
    public string? Type { get; set; } // "Team", "Individual", etc.
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Server> Servers { get; set; } = new List<Server>();
}