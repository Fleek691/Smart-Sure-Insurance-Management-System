namespace SmartSure.PolicyService.DTOs;

/// <summary>
/// Response DTO for the premium calculation endpoint.
/// Shows the customer the monthly cost before they commit to purchasing.
/// </summary>
public class PremiumCalculationDto
{
    public int ProductId { get; set; }
    public decimal CoverageAmount { get; set; }
    public int TermMonths { get; set; }

    /// <summary>Calculated monthly premium in INR.</summary>
    public decimal MonthlyPremium { get; set; }
}