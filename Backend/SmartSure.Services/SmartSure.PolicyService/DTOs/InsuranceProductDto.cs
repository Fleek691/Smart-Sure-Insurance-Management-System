namespace SmartSure.PolicyService.DTOs;

/// <summary>Response DTO for an insurance product shown in the product catalog.</summary>
public class InsuranceProductDto
{
    public int ProductId { get; set; }
    public int TypeId { get; set; }

    /// <summary>Top-level category name, e.g. "Vehicle" or "Home".</summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>Specific product name, e.g. "Maruti Suzuki" or "Apartment".</summary>
    public string SubTypeName { get; set; } = string.Empty;

    /// <summary>Annual base premium in INR — used as the starting point for premium calculation.</summary>
    public decimal BasePremium { get; set; }
}