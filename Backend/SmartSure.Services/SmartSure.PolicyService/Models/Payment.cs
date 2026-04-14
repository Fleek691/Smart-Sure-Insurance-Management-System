namespace SmartSure.PolicyService.Models;

/// <summary>
/// Records a payment transaction made against a policy.
/// Supports both direct purchase and Razorpay payment flows.
/// </summary>
public class Payment
{
    public Guid PaymentId { get; set; }
    public Guid PolicyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }

    /// <summary>Payment method used, e.g. "RAZORPAY" or "DIRECT".</summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Razorpay payment ID or other external transaction reference.</summary>
    public string? TransactionRef { get; set; }

    /// <summary>Payment status — typically "SUCCESS" or "FAILED".</summary>
    public string Status { get; set; } = string.Empty;

    public Policy? Policy { get; set; }
}