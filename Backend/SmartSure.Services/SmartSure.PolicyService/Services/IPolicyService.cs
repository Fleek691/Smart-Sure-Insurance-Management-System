using SmartSure.PolicyService.DTOs;

namespace SmartSure.PolicyService.Services;

/// <summary>
/// Defines policy lifecycle operations: product catalog, premium calculation, purchase, and cancellation.
/// </summary>
public interface IPolicyService
{
    Task<List<InsuranceProductDto>> GetProductsAsync();
    Task<InsuranceProductDto> GetProductByIdAsync(int productId);
    Task<InsuranceProductDto> CreateProductAsync(CreateInsuranceProductDto dto);
    Task<InsuranceProductDto> UpdateProductAsync(int productId, UpdateInsuranceProductDto dto);
    Task DeleteProductAsync(int productId);
    Task<PremiumCalculationDto> CalculatePremiumAsync(int productId, decimal coverageAmount, int termMonths);
    Task<PolicyDto> PurchasePolicyAsync(Guid userId, PurchasePolicyDto dto);
    Task<List<PolicyDto>> GetMyPoliciesAsync(Guid userId);
    Task<PolicyDto> GetPolicyByIdAsync(Guid policyId);
    Task<PolicyDto> CancelPolicyAsync(Guid policyId, Guid userId);
    Task<List<PolicyDto>> GetAllPoliciesForAdminAsync();
    Task<PolicyDto> UpdatePolicyStatusAsync(Guid policyId, string status);
}