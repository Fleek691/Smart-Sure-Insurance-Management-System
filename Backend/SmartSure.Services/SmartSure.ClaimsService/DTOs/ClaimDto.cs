namespace SmartSure.ClaimsService.DTOs;

/// <summary>
/// Response DTO representing a claim — returned to both customers and admins.
/// Customers only see their own claims; admins can see all.
/// </summary>
public class ClaimDto
{
    public Guid ClaimId { get; set; }

    /// <summary>Human-readable reference, e.g. CLM-20260414-4821.</summary>
    public string ClaimNumber { get; set; } = string.Empty;

    public Guid PolicyId { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }

    /// <summary>Current lifecycle status (Draft, Submitted, UnderReview, Approved, Rejected).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Note added by the admin during review, approval, or rejection.</summary>
    public string? AdminNote { get; set; }

    /// <summary>Admin user ID who last acted on this claim.</summary>
    public Guid? ReviewedBy { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}