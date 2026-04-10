namespace SmartSure.PolicyService.DTOs;

public class PremiumCalculationDto
{
    public int ProductId { get; set; }
    public decimal CoverageAmount { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyPremium { get; set; }
}