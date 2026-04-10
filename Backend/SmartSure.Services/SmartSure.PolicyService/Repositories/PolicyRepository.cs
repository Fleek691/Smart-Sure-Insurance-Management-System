using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly PolicyDbContext _context;

    public PolicyRepository(PolicyDbContext context)
    {
        _context = context;
    }

    public Task<Policy?> GetByIdAsync(Guid policyId)
    {
        return _context.Policies
            .Include(x => x.Type)
            .Include(x => x.SubType)
            .FirstOrDefaultAsync(x => x.PolicyId == policyId);
    }

    public Task<List<InsuranceSubType>> GetProductsAsync()
    {
        return _context.InsuranceSubTypes
            .Include(x => x.Type)
            .OrderBy(x => x.Type!.TypeName)
            .ThenBy(x => x.SubTypeName)
            .ToListAsync();
    }

    public Task<InsuranceSubType?> GetProductByIdAsync(int productId)
    {
        return _context.InsuranceSubTypes
            .Include(x => x.Type)
            .FirstOrDefaultAsync(x => x.SubTypeId == productId);
    }

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

    public Task<List<Policy>> GetByUserIdAsync(Guid userId)
    {
        return _context.Policies
            .Include(x => x.Type)
            .Include(x => x.SubType)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

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

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}