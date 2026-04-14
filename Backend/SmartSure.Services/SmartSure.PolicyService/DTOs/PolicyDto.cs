namespace SmartSure.PolicyService.DTOs;

/// <summary>
/// Response DTO for a purchased policy — returned to both customers and admins.
/// </summary>
public class PolicyDto
{
    public Guid PolicyId { get; set; }

    /// <summary>Human-readable reference, e.g. SS-20260414-4821.</summary>
    public string PolicyNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public int TypeId { get; set; }
    public int SubTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string SubTypeName { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the policy was issued.</summary>
    public DateTime IssuedDate { get; set; }

    /// <summary>Date from which coverage begins.</summary>
    public DateOnly InsuranceDate { get; set; }

    public decimal CoverageAmount { get; set; }
    public decimal MonthlyPremium { get; set; }

    /// <summary>Date on which coverage ends.</summary>
    public DateOnly EndDate { get; set; }

    /// <summary>Current lifecycle status (Active, Cancelled, Expired).</summary>
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}