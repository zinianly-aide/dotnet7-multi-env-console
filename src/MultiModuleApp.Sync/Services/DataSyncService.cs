using MultiModuleApp.Infrastructure.Data;
using MultiModuleApp.Sync.Interfaces;
using Serilog;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace MultiModuleApp.Sync.Services;

public class DataSyncService : IDataSyncService
{
    private readonly MySqlDbContext _mysqlContext;
    private readonly IOracleRepository _oracleRepository;
    private readonly string _oracleConnectionString;
    private readonly List<SyncReport> _syncHistory = new();

    public DataSyncService(
        MySqlDbContext mysqlContext,
        IOracleRepository oracleRepository,
        IConfiguration configuration)
    {
        _mysqlContext = mysqlContext;
        _oracleRepository = oracleRepository;
        _oracleConnectionString = configuration.GetConnectionString("OracleConnection") ?? string.Empty;
    }

    public async Task<SyncResult> SyncFromOracleToMySqlAsync(string tableName)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new SyncResult();

        try
        {
            Log.Information("Starting sync from Oracle to MySQL for table: {TableName}", tableName);

            // Read from Oracle
            var oracleData = await _oracleRepository.ExecuteQueryAsync($"SELECT * FROM {tableName}");
            result.RecordsRead = oracleData.Rows.Count;

            Log.Information("Read {Count} records from Oracle", result.RecordsRead);

            // Write to MySQL
            foreach (DataRow row in oracleData.Rows)
            {
                try
                {
                    // This is a simplified example - in production, you'd map columns properly
                    var insertQuery = BuildInsertQuery(tableName, row);
                    await _mysqlContext.Database.ExecuteSqlRawAsync(insertQuery);
                    result.RecordsWritten++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to sync record from Oracle to MySQL: {Error}", ex.Message);
                    result.RecordsFailed++;
                }
            }

            result.Success = true;
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            Log.Information("Sync completed successfully: {Written} records written in {Duration}", 
                result.RecordsWritten, result.Duration);

            _syncHistory.Add(new SyncReport
            {
                SyncTime = result.SyncTime,
                Source = "Oracle",
                Destination = "MySQL",
                TableName = tableName,
                RecordsRead = result.RecordsRead,
                RecordsWritten = result.RecordsWritten,
                RecordsFailed = result.RecordsFailed,
                Success = result.Success
            });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            Log.Error(ex, "Sync from Oracle to MySQL failed: {Error}", ex.Message);
        }

        return result;
    }

    public async Task<SyncResult> SyncFromMySqlToOracleAsync(string tableName)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new SyncResult();

        try
        {
            Log.Information("Starting sync from MySQL to Oracle for table: {TableName}", tableName);

            // Read from MySQL
            var connection = _mysqlContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {tableName}";
            
            using var reader = await command.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);
            result.RecordsRead = table.Rows.Count;

            Log.Information("Read {Count} records from MySQL", result.RecordsRead);

            // Write to Oracle
            foreach (DataRow row in table.Rows)
            {
                try
                {
                    var insertQuery = BuildOracleInsertQuery(tableName, row);
                    await _oracleRepository.ExecuteNonQueryAsync(insertQuery);
                    result.RecordsWritten++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to sync record from MySQL to Oracle: {Error}", ex.Message);
                    result.RecordsFailed++;
                }
            }

            await connection.CloseAsync();

            result.Success = true;
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            Log.Information("Sync completed successfully: {Written} records written in {Duration}",
                result.RecordsWritten, result.Duration);

            _syncHistory.Add(new SyncReport
            {
                SyncTime = result.SyncTime,
                Source = "MySQL",
                Destination = "Oracle",
                TableName = tableName,
                RecordsRead = result.RecordsRead,
                RecordsWritten = result.RecordsWritten,
                RecordsFailed = result.RecordsFailed,
                Success = result.Success
            });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            Log.Error(ex, "Sync from MySQL to Oracle failed: {Error}", ex.Message);
        }

        return result;
    }

    public async Task<SyncResult> BidirectionalSyncAsync(string tableName)
    {
        Log.Information("Starting bidirectional sync for table: {TableName}", tableName);

        var result1 = await SyncFromOracleToMySqlAsync(tableName);
        var result2 = await SyncFromMySqlToOracleAsync(tableName);

        var combinedResult = new SyncResult
        {
            Success = result1.Success && result2.Success,
            RecordsRead = result1.RecordsRead + result2.RecordsRead,
            RecordsWritten = result1.RecordsWritten + result2.RecordsWritten,
            RecordsFailed = result1.RecordsFailed + result2.RecordsFailed,
            Duration = result1.Duration + result2.Duration,
            SyncTime = DateTime.UtcNow,
            ErrorMessage = result1.ErrorMessage ?? result2.ErrorMessage
        };

        return combinedResult;
    }

    public Task<IEnumerable<SyncReport>> GetSyncHistoryAsync()
    {
        return Task.FromResult<IEnumerable<SyncReport>>(_syncHistory.OrderByDescending(r => r.SyncTime));
    }

    private string BuildInsertQuery(string tableName, DataRow row)
    {
        var columns = string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        var values = string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(c => 
            row[c] == DBNull.Value ? "NULL" : $"'{EscapeSqlValue(row[c].ToString())}'"));
        
        return $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
    }

    private string BuildOracleInsertQuery(string tableName, DataRow row)
    {
        var columns = string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        var values = string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(c =>
            row[c] == DBNull.Value ? "NULL" : $"'{EscapeSqlValue(row[c].ToString())}'"));

        return $"INSERT INTO {tableName} ({columns}) VALUES ({values})";
    }

    private string EscapeSqlValue(string value)
    {
        return value.Replace("'", "''");
    }
}
