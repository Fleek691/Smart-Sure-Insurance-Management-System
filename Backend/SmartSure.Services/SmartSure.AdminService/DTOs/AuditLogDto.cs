namespace SmartSure.AdminService.DTOs;

/// <summary>Response DTO for a single audit log entry returned by the paginated audit log endpoint.</summary>
public class AuditLogDto
{
    public Guid LogId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Action constant, e.g. "POLICY_ACTIVATED", "CLAIM_SUBMITTED".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Domain entity type, e.g. "Policy", "Claim", "User".</summary>
    public string EntityType { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    /// <summary>Raw JSON event payload — may be null for older log entries.</summary>
    public string? Details { get; set; }

    public DateTime TimeStamp { get; set; }
}
