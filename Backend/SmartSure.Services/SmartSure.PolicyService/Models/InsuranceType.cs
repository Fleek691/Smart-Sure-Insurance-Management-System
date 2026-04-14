namespace SmartSure.PolicyService.Models;

/// <summary>
/// Top-level insurance category (e.g. "Vehicle", "Home").
/// Each type contains one or more <see cref="InsuranceSubType"/> products.
/// </summary>
public class InsuranceType
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;

    public ICollection<InsuranceSubType> SubTypes { get; set; } = new List<InsuranceSubType>();
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}