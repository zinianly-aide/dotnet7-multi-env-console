using System.Text.Json;
using MultiModuleApp.Audit.Interfaces;
using MultiModuleApp.Audit.Models;
using Serilog;

namespace MultiModuleApp.Audit.Services;

public class AuditService : IAuditService, IDisposable
{
    private readonly string _logFilePath;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public AuditService(string logFilePath)
    {
        _logFilePath = logFilePath;
        EnsureLogDirectoryExists();
    }

    private void EnsureLogDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task LogAsync(AuditEntry entry)
    {
        await _semaphore.WaitAsync();
        try
        {
            var logLine = $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {entry.Level.PadRight(12)} | {entry.Action.PadRight(20)} | {entry.Entity ?? "N/A".PadRight(20)} | {entry.EntityId?.ToString() ?? "N/A".PadRight(10)} | {entry.UserId ?? "System"} | {entry.Message}";
            await File.AppendAllTextAsync(_logFilePath, logLine + Environment.NewLine);

            Log.Information("Audit: {Action} on {Entity} (ID: {EntityId}) by {UserId}", 
                entry.Action, entry.Entity, entry.EntityId, entry.UserId ?? "System");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task LogActionAsync(string action, string? entity = null, int? entityId = null, string? userId = null, string? details = null)
    {
        var entry = new AuditEntry
        {
            Action = action,
            Entity = entity,
            EntityId = entityId,
            UserId = userId,
            Details = details,
            Message = $"{action} performed on {entity ?? "system"}" + (entityId.HasValue ? $" with ID {entityId}" : "")
        };

        await LogAsync(entry);
    }

    public async Task<IEnumerable<AuditEntry>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_logFilePath))
            {
                return Enumerable.Empty<AuditEntry>();
            }

            var lines = await File.ReadAllLinesAsync(_logFilePath);
            var entries = new List<AuditEntry>();

            foreach (var line in lines)
            {
                try
                {
                    var entry = ParseAuditLine(line);
                    if (entry != null)
                    {
                        if ((!startDate.HasValue || entry.Timestamp >= startDate.Value) &&
                            (!endDate.HasValue || entry.Timestamp <= endDate.Value))
                        {
                            entries.Add(entry);
                        }
                    }
                }
                catch
                {
                    // Skip malformed lines
                    continue;
                }
            }

            return entries.OrderByDescending(e => e.Timestamp);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private AuditEntry? ParseAuditLine(string line)
    {
        // Simple parser for the log format
        // Format: timestamp | level | action | entity | entityId | userId | message
        var parts = line.Split('|').Select(p => p.Trim()).ToArray();
        if (parts.Length >= 7)
        {
            return new AuditEntry
            {
                Timestamp = DateTime.TryParse(parts[0], out var timestamp) ? timestamp : DateTime.UtcNow,
                Level = parts[1],
                Action = parts[2],
                Entity = parts[3],
                EntityId = int.TryParse(parts[4], out var entityId) ? entityId : null,
                UserId = parts[5],
                Message = parts[6]
            };
        }
        return null;
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
