namespace SmartSure.PolicyService.Models;

public class PolicyDetail
{
    public Guid PolicyDetailId { get; set; }
    public Guid PolicyId { get; set; }
    public string Details { get; set; } = string.Empty;

    public Policy? Policy { get; set; }
}