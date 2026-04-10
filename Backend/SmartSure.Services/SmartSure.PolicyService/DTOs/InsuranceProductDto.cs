namespace SmartSure.PolicyService.DTOs;

public class InsuranceProductDto
{
    public int ProductId { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string SubTypeName { get; set; } = string.Empty;
    public decimal BasePremium { get; set; }
}