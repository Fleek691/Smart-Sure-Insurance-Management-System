namespace SmartSure.PolicyService.Models;

public class InsuranceSubType
{
    public int SubTypeId { get; set; }
    public int TypeId { get; set; }
    public string SubTypeName { get; set; } = string.Empty;
    public decimal BasePremium { get; set; }

    public InsuranceType? Type { get; set; }
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}