namespace Amdocs.Atlas.Core.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "App Server", "DB Server"
    public string? Description { get; set; }
    public bool IsCritical { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Server> Servers { get; set; } = new List<Server>();
}