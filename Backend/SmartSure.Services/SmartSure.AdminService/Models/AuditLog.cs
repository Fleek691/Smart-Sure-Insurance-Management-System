namespace SmartSure.AdminService.Models;

/// <summary>
/// Persisted audit log entry created by <see cref="Consumers.AuditEventConsumer"/>
/// whenever a domain event is received from any service.
/// Provides the raw data for dashboard stats, reports, and the audit log UI.
/// </summary>
public class AuditLog
{
    public Guid LogId { get; set; }

    /// <summary>The user who triggered the action (customer or admin).</summary>
    public Guid UserId { get; set; }

    /// <summary>Action constant, e.g. "POLICY_ACTIVATED", "CLAIM_SUBMITTED".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Domain entity type, e.g. "Policy", "Claim", "User".</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>String representation of the entity's primary key.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>JSON-serialized event payload — used for revenue extraction and report details.</summary>
    public string? Details { get; set; }

    public DateTime TimeStamp { get; set; }
}
