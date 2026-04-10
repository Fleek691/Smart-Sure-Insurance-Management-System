namespace SmartSure.PolicyService.Models;

public class Payment
{
    public Guid PaymentId { get; set; }
    public Guid PolicyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionRef { get; set; }
    public string Status { get; set; } = string.Empty;

    public Policy? Policy { get; set; }
}