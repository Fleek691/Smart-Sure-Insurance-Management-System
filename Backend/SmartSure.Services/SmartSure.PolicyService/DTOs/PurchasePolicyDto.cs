using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

public class PurchasePolicyDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, double.MaxValue)]
    public decimal CoverageAmount { get; set; }

    [Range(1, 120)]
    public int TermMonths { get; set; }

    public DateTime InsuranceDate { get; set; }
}