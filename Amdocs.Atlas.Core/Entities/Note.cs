namespace Amdocs.Atlas.Core.Entities;

public class Note
{
    public int NoteId { get; set; }
    public int ServerId { get; set; }
    public Server Server { get; set; } = null!;

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Text { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;
}