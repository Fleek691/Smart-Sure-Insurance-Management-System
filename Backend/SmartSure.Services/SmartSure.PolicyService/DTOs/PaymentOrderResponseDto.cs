namespace SmartSure.PolicyService.DTOs;

/// <summary>Returned to the frontend so it can open the Razorpay checkout.</summary>
public class PaymentOrderResponseDto
{
    /// <summary>Razorpay order id — e.g. order_XXXXXXXXXX</summary>
    public string RazorpayOrderId { get; set; } = string.Empty;

    /// <summary>Amount in paise (INR × 100).</summary>
    public long AmountPaise { get; set; }

    /// <summary>Currency — always "INR".</summary>
    public string Currency { get; set; } = "INR";

    /// <summary>Razorpay publishable key — safe to expose to the browser.</summary>
    public string RazorpayKeyId { get; set; } = string.Empty;

    /// <summary>Calculated monthly premium shown to the user before payment.</summary>
    public decimal MonthlyPremium { get; set; }

    /// <summary>Opaque token the frontend echoes back in the verify call.</summary>
    public string PendingOrderToken { get; set; } = string.Empty;
}
