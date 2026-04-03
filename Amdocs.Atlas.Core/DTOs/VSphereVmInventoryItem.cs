namespace Amdocs.Atlas.Core.DTOs;

/// <summary>
/// Minimal VM inventory record returned from vSphere sync.
/// </summary>
public class VSphereVmInventoryItem
{
    public string Name { get; set; } = string.Empty;
    public string? InstanceUuid { get; set; }
    public string? BiosUuid { get; set; }
    public string? GuestOs { get; set; }
    public string? PowerState { get; set; }
    public string? PrimaryIpAddress { get; set; }
    public int CpuCount { get; set; }
    public long MemoryMiB { get; set; }
}
