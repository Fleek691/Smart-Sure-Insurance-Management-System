using System.Net.Http.Headers;
using System.Text.Json;

namespace SmartSure.ClaimsService.Services;

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

    public async Task<string?> GetPolicyStatusAsync(Guid policyId, string bearerToken)
    {
        try
        {
            var policyServiceUrl = _configuration["PolicyService:BaseUrl"]
                ?? "http://localhost:5002";

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{policyServiceUrl.TrimEnd('/')}/api/policies/{policyId}");

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
            _logger.LogWarning(ex, "Failed to verify policy {PolicyId} status from PolicyService", policyId);
            return null;
        }
    }
}
