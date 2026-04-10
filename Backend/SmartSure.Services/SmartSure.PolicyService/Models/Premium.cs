namespace SmartSure.PolicyService.Models;

public class Premium
{
    public Guid PremiumId { get; set; }
    public Guid PolicyId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string Status { get; set; } = string.Empty;

    public Policy? Policy { get; set; }
}