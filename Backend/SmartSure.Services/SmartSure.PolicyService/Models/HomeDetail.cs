namespace SmartSure.PolicyService.Models;

public class HomeDetail
{
    public Guid HomeId { get; set; }
    public Guid PolicyId { get; set; }
    public string Address { get; set; } = string.Empty;
    public int YearBuilt { get; set; }
    public string? ConstructionType { get; set; }

    public Policy? Policy { get; set; }
}