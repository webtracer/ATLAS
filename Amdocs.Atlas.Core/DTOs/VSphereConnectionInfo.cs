namespace Amdocs.Atlas.Core.DTOs;

/// <summary>
/// Connection information for vSphere (without credentials)
/// </summary>
public class VSphereConnectionInfo
{
    public int VcenterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;  // Will be set from Vcenter.Host or IpAddress
    public int Port { get; set; } = 443;
    public bool SslVerify { get; set; }
    public string? Description { get; set; }
}
