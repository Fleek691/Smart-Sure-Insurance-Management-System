using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

public class UpdateInsuranceProductDto
{
    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string SubTypeName { get; set; } = string.Empty;

    [Range(1, double.MaxValue)]
    public decimal BasePremium { get; set; }
}