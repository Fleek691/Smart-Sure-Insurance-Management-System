namespace SmartSure.PolicyService.Models;

/// <summary>
/// Tracks individual premium installments for a policy.
/// Each installment has a due date and a paid/unpaid status.
/// </summary>
public class Premium
{
    public Guid PremiumId { get; set; }
    public Guid PolicyId { get; set; }
    public decimal Amount { get; set; }

    /// <summary>Date by which this installment must be paid.</summary>
    public DateOnly DueDate { get; set; }

    /// <summary>UTC timestamp when the premium was paid — null if still outstanding.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Installment status, e.g. "PENDING", "PAID", "OVERDUE".</summary>
    public string Status { get; set; } = string.Empty;

    public Policy? Policy { get; set; }
}