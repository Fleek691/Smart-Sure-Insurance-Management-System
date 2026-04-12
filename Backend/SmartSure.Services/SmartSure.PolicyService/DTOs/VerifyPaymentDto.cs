namespace SmartSure.PolicyService.DTOs;

/// <summary>
/// Sent by the frontend after the Razorpay checkout succeeds.
/// Contains the three values Razorpay returns to the browser on success.
/// </summary>
public class VerifyPaymentDto
{
    /// <summary>Opaque token returned by create-order — ties this verification to the pending purchase.</summary>
    public string PendingOrderToken { get; set; } = string.Empty;

    /// <summary>razorpay_order_id from the checkout response.</summary>
    public string RazorpayOrderId { get; set; } = string.Empty;

    /// <summary>razorpay_payment_id from the checkout response.</summary>
    public string RazorpayPaymentId { get; set; } = string.Empty;

    /// <summary>razorpay_signature from the checkout response.</summary>
    public string RazorpaySignature { get; set; } = string.Empty;
}
