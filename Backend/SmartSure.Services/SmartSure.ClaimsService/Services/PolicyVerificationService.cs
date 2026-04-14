using System.Net.Http.Headers;
using System.Text.Json;

namespace SmartSure.ClaimsService.Services;

/// <summary>
/// HTTP client implementation of <see cref="IPolicyVerificationService"/>.
/// Calls the PolicyService REST API to check a policy's status before allowing a claim.
/// Returns null (non-fatal) if the PolicyService is unreachable, letting the claim proceed.
/// </summary>
public class PolicyVerificationService : IPolicyVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PolicyVerificationService> _logger;

    public PolicyVerificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PolicyVerificationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// GETs /api/policies/{policyId} from the PolicyService using the caller's bearer token,
    /// then extracts the "status" field from the JSON response.
    /// Returns null if the request fails or the policy is not found.
    /// </summary>
    public async Task<string?> GetPolicyStatusAsync(Guid policyId, string bearerToken)
    {
        try
        {
            var policyServiceUrl = _configuration["PolicyService:BaseUrl"]
                ?? "http://localhost:5002";

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{policyServiceUrl.TrimEnd('/')}/api/policies/{policyId}");

            // Forward the caller's JWT so the PolicyService can authorize the request
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PolicyService returned {StatusCode} for policy {PolicyId}",
                    response.StatusCode, policyId);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("status", out var statusProp))
                return statusProp.GetString();

            return null;
        }
        catch (Exception ex)
        {
            // Non-fatal — log and return null so the claim can still be created
            _logger.LogWarning(ex, "Failed to verify policy {PolicyId} status from PolicyService", policyId);
            return null;
        }
    }
}
