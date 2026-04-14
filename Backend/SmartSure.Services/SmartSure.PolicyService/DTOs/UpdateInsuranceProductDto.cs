using System.ComponentModel.DataAnnotations;

namespace SmartSure.PolicyService.DTOs;

/// <summary>Request body for the admin update-product endpoint.</summary>
public class UpdateInsuranceProductDto
{
    /// <summary>New or existing category name for the product.</summary>
    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string SubTypeName { get; set; } = string.Empty;

    /// <summary>Updated annual base premium in INR.</summary>
    [Range(1, double.MaxValue)]
    public decimal BasePremium { get; set; }
}