using SmartSure.ClaimsService.DTOs;

namespace SmartSure.ClaimsService.Services;

/// <summary>
/// Defines customer-facing claim operations: creation, submission, and document management.
/// </summary>
public interface IClaimService
{
    Task<ClaimDto> CreateClaimAsync(Guid userId, CreateClaimDto dto, string bearerToken = "");
    Task<List<ClaimDto>> GetMyClaimsAsync(Guid userId);
    Task<ClaimDto> GetClaimAsync(Guid claimId, Guid userId, bool isAdmin);
    Task<ClaimDto> SubmitClaimAsync(Guid claimId, Guid userId);
    Task<ClaimDocumentDto> UploadDocumentAsync(Guid claimId, Guid userId, UploadClaimDocumentDto dto);
    Task<List<ClaimDocumentDto>> GetDocumentsAsync(Guid claimId, Guid userId, bool isAdmin);
    Task DeleteDocumentAsync(Guid claimId, Guid docId, Guid userId);
}