namespace SmartSure.PolicyService.Models;

/// <summary>
/// Stores arbitrary JSON details for a policy (e.g. beneficiary info, custom fields).
/// One-to-one with <see cref="Policy"/> — only created when extra detail is needed.
/// </summary>
public class PolicyDetail
{
    public Guid PolicyDetailId { get; set; }
    public Guid PolicyId { get; set; }

    /// <summary>Serialized JSON blob containing policy-specific metadata.</summary>
    public string Details { get; set; } = string.Empty;

    public Policy? Policy { get; set; }
}