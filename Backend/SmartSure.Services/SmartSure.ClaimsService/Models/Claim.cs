namespace SmartSure.ClaimsService.Models;

/// <summary>
/// Represents an insurance claim filed by a customer against an active policy.
/// Follows a state machine: Draft → Submitted → UnderReview → Approved | Rejected.
/// </summary>
public class Claim
{
    public Guid ClaimId { get; set; }

    /// <summary>Human-readable claim reference, e.g. CLM-20260414-4821.</summary>
    public string ClaimNumber { get; set; } = string.Empty;

    /// <summary>The policy this claim is filed against.</summary>
    public Guid PolicyId { get; set; }

    /// <summary>The customer who filed the claim.</summary>
    public Guid UserId { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }

    /// <summary>Current lifecycle status — see <see cref="SmartSure.Shared.Constants.ClaimStatus"/>.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Optional note left by the admin during review, approval, or rejection.</summary>
    public string? AdminNote { get; set; }

    /// <summary>Admin user ID who last reviewed this claim.</summary>
    public Guid? ReviewedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    /// <summary>Set when the customer transitions the claim from Draft to Submitted.</summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Set when an admin takes an action (approve/reject/review).</summary>
    public DateTime? ReviewedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // ── Navigation properties ──────────────────────────────────────────────
    public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
    public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();
}