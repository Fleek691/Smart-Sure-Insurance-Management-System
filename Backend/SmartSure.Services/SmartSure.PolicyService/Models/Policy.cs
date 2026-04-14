namespace SmartSure.PolicyService.Models;

/// <summary>
/// Represents a purchased insurance policy.
/// Follows a lifecycle: Active → Cancelled | Expired.
/// </summary>
public class Policy
{
    public Guid PolicyId { get; set; }

    /// <summary>Human-readable policy reference, e.g. SS-20260414-4821.</summary>
    public string PolicyNumber { get; set; } = string.Empty;

    /// <summary>The customer who owns this policy.</summary>
    public Guid UserId { get; set; }

    public int TypeId { get; set; }
    public int SubTypeId { get; set; }

    /// <summary>Current lifecycle status — see <see cref="SmartSure.Shared.Constants.PolicyStatus"/>.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the policy record was created in the system.</summary>
    public DateTime IssuedDate { get; set; }

    /// <summary>The date from which coverage begins (chosen by the customer).</summary>
    public DateOnly InsuranceDate { get; set; }

    /// <summary>The date on which coverage ends (InsuranceDate + TermMonths).</summary>
    public DateOnly EndDate { get; set; }

    public decimal CoverageAmount { get; set; }
    public decimal MonthlyPremium { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation properties ──────────────────────────────────────────────
    public InsuranceType? Type { get; set; }
    public InsuranceSubType? SubType { get; set; }
    public PolicyDetail? PolicyDetail { get; set; }
    public VehicleDetail? VehicleDetail { get; set; }
    public HomeDetail? HomeDetail { get; set; }
    public ICollection<Premium> Premiums { get; set; } = new List<Premium>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}