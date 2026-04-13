namespace SmartSure.ClaimsService.Services;

/// <summary>
/// Verifies policy status by calling PolicyService via HTTP.
/// Keeps ClaimsService decoupled from PolicyDb.
/// </summary>
public interface IPolicyVerificationService
{
    /// <summary>
    /// Returns the status string of the policy (e.g. "ACTIVE", "CANCELLED").
    /// Returns null if the policy cannot be reached or does not exist.
    /// </summary>
    Task<string?> GetPolicyStatusAsync(Guid policyId, string bearerToken);
}
