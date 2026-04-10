using Microsoft.Extensions.Caching.Memory;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Events;
using SmartSure.Shared.Exceptions;

namespace SmartSure.PolicyService.Services;

public class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly IPolicyEventPublisher _eventPublisher;

    public PolicyService(
        IPolicyRepository policyRepository,
        IMemoryCache memoryCache,
        IPolicyEventPublisher eventPublisher)
    {
        _policyRepository = policyRepository;
        _memoryCache = memoryCache;
        _eventPublisher = eventPublisher;
    }

    public async Task<List<InsuranceProductDto>> GetProductsAsync()
    {
        if (_memoryCache.TryGetValue("insurance_types_all", out List<InsuranceProductDto>? cachedProducts) && cachedProducts is not null)
        {
            return cachedProducts;
        }

        var products = await _policyRepository.GetProductsAsync();
        var result = new List<InsuranceProductDto>();

        foreach (var product in products)
        {
            result.Add(MapProduct(product));
        }

        _memoryCache.Set("insurance_types_all", result, TimeSpan.FromMinutes(60));
        return result;
    }

    public async Task<InsuranceProductDto> GetProductByIdAsync(int productId)
    {
        var product = await _policyRepository.GetProductByIdAsync(productId)
                      ?? throw new NotFoundException("Product not found.");

        return MapProduct(product);
    }

    public async Task<InsuranceProductDto> CreateProductAsync(CreateInsuranceProductDto dto)
    {
        var type = await _policyRepository.GetTypeByNameAsync(dto.TypeName.Trim());
        if (type is null)
        {
            type = new InsuranceType
            {
                TypeName = dto.TypeName.Trim()
            };

            await _policyRepository.AddTypeAsync(type);
            await _policyRepository.SaveChangesAsync();
        }

        var product = new InsuranceSubType
        {
            TypeId = type.TypeId,
            SubTypeName = dto.SubTypeName.Trim(),
            BasePremium = dto.BasePremium,
            Type = type
        };

        await _policyRepository.AddProductAsync(product);
        await _policyRepository.SaveChangesAsync();
        _memoryCache.Remove("insurance_types_all");

        return MapProduct(product);
    }

    public async Task<InsuranceProductDto> UpdateProductAsync(int productId, UpdateInsuranceProductDto dto)
    {
        var product = await _policyRepository.GetProductByIdAsync(productId)
                      ?? throw new NotFoundException("Product not found.");

        var type = await _policyRepository.GetTypeByNameAsync(dto.TypeName.Trim());
        if (type is null)
        {
            type = new InsuranceType
            {
                TypeName = dto.TypeName.Trim()
            };

            await _policyRepository.AddTypeAsync(type);
            await _policyRepository.SaveChangesAsync();
        }

        product.TypeId = type.TypeId;
        product.Type = type;
        product.SubTypeName = dto.SubTypeName.Trim();
        product.BasePremium = dto.BasePremium;

        await _policyRepository.SaveChangesAsync();
        _memoryCache.Remove("insurance_types_all");

        return MapProduct(product);
    }

    public async Task DeleteProductAsync(int productId)
    {
        var product = await _policyRepository.GetProductByIdAsync(productId)
                      ?? throw new NotFoundException("Product not found.");

        await _policyRepository.RemoveProductAsync(product);
        await _policyRepository.SaveChangesAsync();
        _memoryCache.Remove("insurance_types_all");
    }

    public async Task<PremiumCalculationDto> CalculatePremiumAsync(int productId, decimal coverageAmount, int termMonths)
    {
        var product = await _policyRepository.GetProductByIdAsync(productId)
                      ?? throw new NotFoundException("Product not found.");

        if (termMonths <= 0)
        {
            throw new ValidationException("Term months must be greater than zero.");
        }

        if (coverageAmount <= 0)
        {
            throw new ValidationException("Coverage amount must be greater than zero.");
        }

        var coverageFactor = coverageAmount / 100000m;
        var termFactor = termMonths / 12m;
        var monthlyPremium = Math.Round(product.BasePremium * coverageFactor * Math.Max(termFactor, 0.5m), 2);

        return new PremiumCalculationDto
        {
            ProductId = product.SubTypeId,
            CoverageAmount = coverageAmount,
            TermMonths = termMonths,
            MonthlyPremium = monthlyPremium
        };
    }

    public async Task<PolicyDto> PurchasePolicyAsync(Guid userId, PurchasePolicyDto dto)
    {
        var premiumResult = await CalculatePremiumAsync(dto.ProductId, dto.CoverageAmount, dto.TermMonths);
        var product = await _policyRepository.GetProductByIdAsync(dto.ProductId)
                      ?? throw new NotFoundException("Product not found.");

        var issuedDate = DateTime.UtcNow;
        var insuranceDate = DateOnly.FromDateTime(dto.InsuranceDate.Date);
        var policy = new Policy
        {
            PolicyId = Guid.NewGuid(),
            PolicyNumber = $"SS-{issuedDate:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            UserId = userId,
            TypeId = product.TypeId,
            SubTypeId = product.SubTypeId,
            IssuedDate = issuedDate,
            InsuranceDate = insuranceDate,
            EndDate = insuranceDate.AddMonths(dto.TermMonths),
            CoverageAmount = dto.CoverageAmount,
            MonthlyPremium = premiumResult.MonthlyPremium,
            Status = PolicyStatus.Active,
            CreatedAt = issuedDate,
            UpdatedAt = issuedDate
        };

        await _policyRepository.AddAsync(policy);
        await _policyRepository.SaveChangesAsync();
        InvalidatePolicyCache(policy.PolicyId, policy.UserId);

        await _eventPublisher.PublishActivatedAsync(new PolicyActivatedEvent
        {
            PolicyId = policy.PolicyId,
            UserId = policy.UserId,
            PolicyNumber = policy.PolicyNumber,
            ActivatedAtUtc = DateTime.UtcNow
        });

        return MapPolicy(policy);
    }

    public async Task<List<PolicyDto>> GetMyPoliciesAsync(Guid userId)
    {
        var cacheKey = GetUserPoliciesCacheKey(userId);
        if (_memoryCache.TryGetValue(cacheKey, out List<PolicyDto>? cachedPolicies) && cachedPolicies is not null)
        {
            return cachedPolicies;
        }

        var policies = await _policyRepository.GetByUserIdAsync(userId);
        var result = new List<PolicyDto>();

        foreach (var policy in policies)
        {
            result.Add(MapPolicy(policy));
        }

        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
        return result;
    }

    public async Task<PolicyDto> GetPolicyByIdAsync(Guid policyId)
    {
        var cacheKey = GetPolicyCacheKey(policyId);
        if (_memoryCache.TryGetValue(cacheKey, out PolicyDto? cachedPolicy) && cachedPolicy is not null)
        {
            return cachedPolicy;
        }

        var policy = await _policyRepository.GetByIdAsync(policyId)
                     ?? throw new NotFoundException("Policy not found.");

        var result = MapPolicy(policy);
        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<PolicyDto> CancelPolicyAsync(Guid policyId, Guid userId)
    {
        var policy = await _policyRepository.GetByIdAsync(policyId)
                     ?? throw new NotFoundException("Policy not found.");

        if (policy.UserId != userId)
        {
            throw new UnauthorizedException("You are not allowed to cancel this policy.");
        }

        if (policy.Status == PolicyStatus.Cancelled)
        {
            throw new ValidationException("Policy is already cancelled.");
        }

        policy.Status = PolicyStatus.Cancelled;
        policy.UpdatedAt = DateTime.UtcNow;

        await _policyRepository.SaveChangesAsync();
        InvalidatePolicyCache(policy.PolicyId, policy.UserId);

        await _eventPublisher.PublishCancelledAsync(new PolicyCancelledEvent
        {
            PolicyId = policy.PolicyId,
            UserId = policy.UserId,
            PolicyNumber = policy.PolicyNumber,
            Reason = "Requested by customer",
            CancelledAtUtc = DateTime.UtcNow
        });

        return MapPolicy(policy);
    }

    public async Task<List<PolicyDto>> GetAllPoliciesForAdminAsync()
    {
        var policies = await _policyRepository.GetAllAsync();
        var result = new List<PolicyDto>();

        foreach (var policy in policies)
        {
            result.Add(MapPolicy(policy));
        }

        return result;
    }

    public async Task<PolicyDto> UpdatePolicyStatusAsync(Guid policyId, string status)
    {
        var nextStatus = status.Trim().ToUpperInvariant();
        if (nextStatus != PolicyStatus.Draft && nextStatus != PolicyStatus.Active &&
            nextStatus != PolicyStatus.Expired && nextStatus != PolicyStatus.Cancelled)
        {
            throw new ValidationException("Invalid policy status.");
        }

        var policy = await _policyRepository.GetByIdAsync(policyId)
                     ?? throw new NotFoundException("Policy not found.");

        policy.Status = nextStatus;
        if (nextStatus != PolicyStatus.Cancelled)
        {
            // No cancellation reason column in architecture policy table.
        }

        policy.UpdatedAt = DateTime.UtcNow;
        await _policyRepository.SaveChangesAsync();
        InvalidatePolicyCache(policy.PolicyId, policy.UserId);

        if (nextStatus == PolicyStatus.Active)
        {
            await _eventPublisher.PublishActivatedAsync(new PolicyActivatedEvent
            {
                PolicyId = policy.PolicyId,
                UserId = policy.UserId,
                PolicyNumber = policy.PolicyNumber,
                ActivatedAtUtc = DateTime.UtcNow
            });
        }
        else if (nextStatus == PolicyStatus.Cancelled)
        {
            await _eventPublisher.PublishCancelledAsync(new PolicyCancelledEvent
            {
                PolicyId = policy.PolicyId,
                UserId = policy.UserId,
                PolicyNumber = policy.PolicyNumber,
                Reason = "Cancelled by admin",
                CancelledAtUtc = DateTime.UtcNow
            });
        }

        return MapPolicy(policy);
    }

    private void InvalidatePolicyCache(Guid policyId, Guid userId)
    {
        _memoryCache.Remove(GetPolicyCacheKey(policyId));
        _memoryCache.Remove(GetUserPoliciesCacheKey(userId));
    }

    private static string GetPolicyCacheKey(Guid policyId)
    {
        return $"policy_{policyId}";
    }

    private static string GetUserPoliciesCacheKey(Guid userId)
    {
        return $"user_policies_{userId}";
    }

    private static InsuranceProductDto MapProduct(InsuranceSubType product)
    {
        return new InsuranceProductDto
        {
            ProductId = product.SubTypeId,
            TypeId = product.TypeId,
            TypeName = product.Type?.TypeName ?? string.Empty,
            SubTypeName = product.SubTypeName,
            BasePremium = product.BasePremium
        };
    }

    private static PolicyDto MapPolicy(Policy policy)
    {
        return new PolicyDto
        {
            PolicyId = policy.PolicyId,
            PolicyNumber = policy.PolicyNumber,
            UserId = policy.UserId,
            TypeId = policy.TypeId,
            SubTypeId = policy.SubTypeId,
            TypeName = policy.Type?.TypeName ?? string.Empty,
            SubTypeName = policy.SubType?.SubTypeName ?? string.Empty,
            IssuedDate = policy.IssuedDate,
            InsuranceDate = policy.InsuranceDate,
            CoverageAmount = policy.CoverageAmount,
            MonthlyPremium = policy.MonthlyPremium,
            EndDate = policy.EndDate,
            Status = policy.Status,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }
}
