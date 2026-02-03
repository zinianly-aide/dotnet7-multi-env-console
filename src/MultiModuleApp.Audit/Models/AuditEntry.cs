namespace MultiModuleApp.Audit.Models;

public class AuditEntry
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "Information";
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? Entity { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
}
