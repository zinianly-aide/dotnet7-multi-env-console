using MultiModuleApp.Audit.Models;

namespace MultiModuleApp.Audit.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditEntry entry);
    Task LogActionAsync(string action, string? entity = null, int? entityId = null, string? userId = null, string? details = null);
    Task<IEnumerable<AuditEntry>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
