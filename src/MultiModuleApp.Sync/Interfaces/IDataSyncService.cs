namespace MultiModuleApp.Sync.Interfaces;

public interface IDataSyncService
{
    Task<SyncResult> SyncFromOracleToMySqlAsync(string tableName);
    Task<SyncResult> SyncFromMySqlToOracleAsync(string tableName);
    Task<SyncResult> BidirectionalSyncAsync(string tableName);
    Task<IEnumerable<SyncReport>> GetSyncHistoryAsync();
}

public class SyncResult
{
    public bool Success { get; set; }
    public int RecordsRead { get; set; }
    public int RecordsWritten { get; set; }
    public int RecordsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime SyncTime { get; set; } = DateTime.UtcNow;
}

public class SyncReport
{
    public DateTime SyncTime { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int RecordsRead { get; set; }
    public int RecordsWritten { get; set; }
    public int RecordsFailed { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
