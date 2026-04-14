using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

/// <summary>Request body for the admin create-product endpoint.</summary>
public class CreateInsuranceProductDto
{
    /// <summary>Top-level category name — auto-created if it doesn't already exist.</summary>
    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>Product name within the category, e.g. "Maruti Suzuki".</summary>
    [Required]
    [MaxLength(150)]
    public string SubTypeName { get; set; } = string.Empty;

    /// <summary>Annual base premium in INR.</summary>
    [Range(1, double.MaxValue)]
    public decimal BasePremium { get; set; }
}