namespace SmartSure.PolicyService.DTOs;

public class PolicyDto
{
    public Guid PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public int TypeId { get; set; }
    public int SubTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string SubTypeName { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateOnly InsuranceDate { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal MonthlyPremium { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}