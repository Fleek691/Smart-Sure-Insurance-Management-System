using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _context;

    public ClaimRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public Task<Claim?> GetByIdAsync(Guid claimId)
    {
        return _context.Claims
            .Include(x => x.Documents)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.ClaimId == claimId);
    }

    public Task<List<Claim>> GetByUserIdAsync(Guid userId)
    {
        return _context.Claims
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public Task<List<Claim>> GetAllAsync()
    {
        return _context.Claims
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public async Task AddAsync(Claim claim)
    {
        await _context.Claims.AddAsync(claim);
    }

    public Task<List<ClaimDocument>> GetDocumentsAsync(Guid claimId)
    {
        return _context.ClaimDocuments
            .Where(x => x.ClaimId == claimId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();
    }

    public Task<ClaimDocument?> GetDocumentByIdAsync(Guid claimId, Guid docId)
    {
        return _context.ClaimDocuments.FirstOrDefaultAsync(x => x.ClaimId == claimId && x.DocId == docId);
    }

    public async Task AddDocumentAsync(ClaimDocument document)
    {
        await _context.ClaimDocuments.AddAsync(document);
    }

    public Task RemoveDocumentAsync(ClaimDocument document)
    {
        _context.ClaimDocuments.Remove(document);
        return Task.CompletedTask;
    }

    public async Task AddStatusHistoryAsync(ClaimStatusHistory statusHistory)
    {
        await _context.ClaimStatusHistory.AddAsync(statusHistory);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}