using SmartSure.PolicyService.DTOs;

namespace SmartSure.PolicyService.Services;

public interface IRazorpayService
{
    /// <summary>
    /// Creates a Razorpay order and caches the pending purchase details.
    /// Returns the data the frontend needs to open the checkout.
    /// </summary>
    Task<PaymentOrderResponseDto> CreateOrderAsync(Guid userId, CreatePaymentOrderDto dto);

    /// <summary>
    /// Verifies the Razorpay HMAC signature, activates the policy, and records the payment.
    /// Returns the newly created policy.
    /// </summary>
    Task<PolicyDto> VerifyAndActivateAsync(Guid userId, VerifyPaymentDto dto);
}
