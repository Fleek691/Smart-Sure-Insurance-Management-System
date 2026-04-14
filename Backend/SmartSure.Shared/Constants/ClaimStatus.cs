namespace SmartSure.Shared.Constants;

/// <summary>
/// Defines the valid lifecycle states for insurance claims.
/// </summary>
public static class ClaimStatus
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";
    public const string UnderReview = "UNDER_REVIEW";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
    public const string Closed = "CLOSED";
}
