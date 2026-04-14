using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPolicyRepository"/>.
/// Handles persistence for insurance types, products, policies, and payments.
/// </summary>
public class PolicyRepository : IPolicyRepository
{
    private readonly PolicyDbContext _context;

    public PolicyRepository(PolicyDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns a policy by ID with its type and sub-type eagerly loaded.</summary>
    public Task<Policy?> GetByIdAsync(Guid policyId)
    {
        return _context.Policies
            .Include(x => x.Type)
            .Include(x => x.SubType)
            .FirstOrDefaultAsync(x => x.PolicyId == policyId);
    }

    /// <summary>Returns all insurance products (sub-types) sorted by type then name.</summary>
    public Task<List<InsuranceSubType>> GetProductsAsync()
    {
        return _context.InsuranceSubTypes
            .Include(x => x.Type)
            .OrderBy(x => x.Type!.TypeName)
            .ThenBy(x => x.SubTypeName)
            .ToListAsync();
    }

    /// <summary>Returns a single insurance product by its sub-type ID.</summary>
    public Task<InsuranceSubType?> GetProductByIdAsync(int productId)
    {
        return _context.InsuranceSubTypes
            .Include(x => x.Type)
            .FirstOrDefaultAsync(x => x.SubTypeId == productId);
    }

    /// <summary>Finds an insurance type by name (case-insensitive).</summary>
    public Task<InsuranceType?> GetTypeByNameAsync(string typeName)
    {
        return _context.InsuranceTypes.FirstOrDefaultAsync(x => x.TypeName.ToLower() == typeName.ToLower());
    }

    public async Task AddTypeAsync(InsuranceType type)
    {
        await _context.InsuranceTypes.AddAsync(type);
    }

    public async Task AddProductAsync(InsuranceSubType product)
    {
        await _context.InsuranceSubTypes.AddAsync(product);
    }

    public Task RemoveProductAsync(InsuranceSubType product)
    {
        _context.InsuranceSubTypes.Remove(product);
        return Task.CompletedTask;
    }

    /// <summary>Returns all policies for a specific user, newest first.</summary>
    public Task<List<Policy>> GetByUserIdAsync(Guid userId)
    {
        return _context.Policies
            .Include(x => x.Type)
            .Include(x => x.SubType)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <summary>Returns all policies across all users, newest first (admin use).</summary>
    public Task<List<Policy>> GetAllAsync()
    {
        return _context.Policies
            .Include(x => x.Type)
            .Include(x => x.SubType)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Policy policy)
    {
        await _context.Policies.AddAsync(policy);
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}