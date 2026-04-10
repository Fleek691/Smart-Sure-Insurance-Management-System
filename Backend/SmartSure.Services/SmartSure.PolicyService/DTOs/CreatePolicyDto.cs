using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

public class CreatePolicyDto
{
    [Required]
    [MaxLength(64)]
    public string PolicyType { get; set; } = string.Empty;

    [Range(1, double.MaxValue)]
    public decimal PremiumAmount { get; set; }

    [Range(1, double.MaxValue)]
    public decimal CoverageAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}