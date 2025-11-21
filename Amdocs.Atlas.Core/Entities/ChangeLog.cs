namespace Amdocs.Atlas.Core.Entities;

public class ChangeLog
{
    public int ChangeLogId { get; set; }

    public string EntityName { get; set; } = string.Empty; // "Server", "Environment", etc.
    public int EntityId { get; set; }

    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}