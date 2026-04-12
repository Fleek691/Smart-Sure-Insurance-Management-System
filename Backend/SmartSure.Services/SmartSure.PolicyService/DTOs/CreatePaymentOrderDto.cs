namespace SmartSure.PolicyService.DTOs;

/// <summary>Request body for creating a Razorpay order before checkout.</summary>
public class CreatePaymentOrderDto
{
    public int ProductId { get; set; }
    public decimal CoverageAmount { get; set; }
    public int TermMonths { get; set; }
    public DateTime InsuranceDate { get; set; }
}
