using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Repositories;

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(Guid claimId);
    Task<List<Claim>> GetByUserIdAsync(Guid userId);
    Task<List<Claim>> GetAllAsync();
    Task AddAsync(Claim claim);

    Task<List<ClaimDocument>> GetDocumentsAsync(Guid claimId);
    Task<ClaimDocument?> GetDocumentByIdAsync(Guid claimId, Guid docId);
    Task AddDocumentAsync(ClaimDocument document);
    Task RemoveDocumentAsync(ClaimDocument document);

    Task AddStatusHistoryAsync(ClaimStatusHistory statusHistory);
    Task SaveChangesAsync();
}