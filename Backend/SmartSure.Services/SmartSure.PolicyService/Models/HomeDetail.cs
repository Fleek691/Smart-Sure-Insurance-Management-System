namespace SmartSure.PolicyService.Models;

/// <summary>
/// Home-specific details captured at policy purchase time.
/// One-to-one with <see cref="Policy"/> — only present for Home insurance policies.
/// </summary>
public class HomeDetail
{
    public Guid HomeId { get; set; }
    public Guid PolicyId { get; set; }

    /// <summary>Full address of the insured property.</summary>
    public string Address { get; set; } = string.Empty;

    public int YearBuilt { get; set; }

    /// <summary>Construction material type, e.g. "Brick", "Wood", "RCC" — optional.</summary>
    public string? ConstructionType { get; set; }

    public Policy? Policy { get; set; }
}