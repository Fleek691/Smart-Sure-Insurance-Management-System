namespace SmartSure.PolicyService.Models;

/// <summary>
/// A specific insurance product within a type (e.g. "Maruti Suzuki" under "Vehicle").
/// Holds the base premium used to calculate the customer's monthly premium.
/// </summary>
public class InsuranceSubType
{
    public int SubTypeId { get; set; }
    public int TypeId { get; set; }
    public string SubTypeName { get; set; } = string.Empty;

    /// <summary>Annual base premium in INR — multiplied by coverage and term factors at purchase time.</summary>
    public decimal BasePremium { get; set; }

    public InsuranceType? Type { get; set; }
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}