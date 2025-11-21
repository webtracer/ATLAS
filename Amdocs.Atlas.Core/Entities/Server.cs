namespace Amdocs.Atlas.Core.Entities;

public class Server
{
    public int ServerId { get; set; }

    public string Hostname { get; set; } = string.Empty;
    public string? Fqdn { get; set; }
    public string? IpAddress { get; set; }
    public string? SecondaryIp { get; set; }

    public string? OsName { get; set; }        // ex: "Microsoft Windows Server 2016 or later (64-bit)"
    public string? OsFamily { get; set; }      // ex: "Windows", "Linux"
    public string? OsMajorVersion { get; set; } // ex: "2016", "2008 R2"

    public bool IsVm { get; set; } = true;
    public string? SourceType { get; set; }    // ex: "VMware", "Azure", "Physical"
    public string? VmInstanceUuid { get; set; }
    public string? VmBiosUuid { get; set; }

    public bool IsActive { get; set; } = true;
    public string? LifecycleStatus { get; set; }    // "In Service", "Retired", etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CommissionedAt { get; set; }
    public DateTime? DecommissionedAt { get; set; }

    // FKs
    public int EnvironmentId { get; set; }
    public Environment? Environment { get; set; } //= null!;

    public int RoleId { get; set; }
    public Role? Role { get; set; } //= null!;

    public int? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? VcenterId { get; set; }
    public Vcenter? Vcenter { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    // Navigation collections
    public ICollection<ServerTag> Tags { get; set; } = new List<ServerTag>();
    public ICollection<HealthCheck> HealthChecks { get; set; } = new List<HealthCheck>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}