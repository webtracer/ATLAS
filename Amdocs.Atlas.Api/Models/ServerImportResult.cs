namespace Amdocs.Atlas.Api.Models;

public class ServerImportResult
{
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public List<ServerImportRowError> Errors { get; set; } = new();
}

public class ServerImportRowError
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}