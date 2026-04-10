namespace SmartSure.PolicyService.Models;

public class InsuranceType
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;

    public ICollection<InsuranceSubType> SubTypes { get; set; } = new List<InsuranceSubType>();
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}