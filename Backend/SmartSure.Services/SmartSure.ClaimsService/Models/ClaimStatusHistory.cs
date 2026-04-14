namespace SmartSure.ClaimsService.Models;

/// <summary>
/// Immutable audit record of every status transition on a claim.
/// Provides a full history trail for compliance and admin review.
/// </summary>
public class ClaimStatusHistory
{
    public Guid HistoryId { get; set; }
    public Guid ClaimId { get; set; }

    /// <summary>Status before the transition.</summary>
    public string OldStatus { get; set; } = string.Empty;

    /// <summary>Status after the transition.</summary>
    public string NewStatus { get; set; } = string.Empty;

    /// <summary>User ID (customer or admin) who triggered the transition.</summary>
    public Guid ChangedBy { get; set; }

    public DateTime ChangedDate { get; set; }

    /// <summary>Optional reason or admin note recorded at the time of the transition.</summary>
    public string? Note { get; set; }

    public Claim? Claim { get; set; }
}