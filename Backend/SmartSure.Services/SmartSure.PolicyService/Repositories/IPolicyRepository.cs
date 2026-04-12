using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories;

public interface IPolicyRepository
{
    Task<List<InsuranceSubType>> GetProductsAsync();
    Task<InsuranceSubType?> GetProductByIdAsync(int productId);
    Task<InsuranceType?> GetTypeByNameAsync(string typeName);
    Task AddTypeAsync(InsuranceType type);
    Task AddProductAsync(InsuranceSubType product);
    Task RemoveProductAsync(InsuranceSubType product);
    Task<Policy?> GetByIdAsync(Guid policyId);
    Task<List<Policy>> GetByUserIdAsync(Guid userId);
    Task<List<Policy>> GetAllAsync();
    Task AddAsync(Policy policy);
    Task AddPaymentAsync(Payment payment);
    Task SaveChangesAsync();
}