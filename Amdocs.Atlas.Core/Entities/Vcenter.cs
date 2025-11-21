namespace Amdocs.Atlas.Core.Entities;

public class Vcenter
{
    public int VcenterId { get; set; }
    public string? Name { get; set; }  // optional friendly name
    public string IpAddress { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Server> Servers { get; set; } = new List<Server>();
}