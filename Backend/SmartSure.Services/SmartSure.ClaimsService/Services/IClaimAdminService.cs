using SmartSure.ClaimsService.DTOs;

namespace SmartSure.ClaimsService.Services;

public interface IClaimAdminService
{
    Task<List<ClaimDto>> GetAllClaimsForAdminAsync();
    Task<ClaimDto> MarkUnderReviewAsync(Guid claimId, Guid adminUserId, string? note);
    Task<ClaimDto> ApproveClaimAsync(Guid claimId, Guid adminUserId, decimal? approvedAmount, string? note);
    Task<ClaimDto> RejectClaimAsync(Guid claimId, Guid adminUserId, string reason);
}
