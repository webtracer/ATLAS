namespace Amdocs.Atlas.Core.Entities;

public class ServerTag
{
    public int ServerTagId { get; set; }
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;

    public string Tag { get; set; } = string.Empty;
}