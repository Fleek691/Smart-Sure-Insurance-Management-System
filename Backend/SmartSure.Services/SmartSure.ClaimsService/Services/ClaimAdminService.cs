using Microsoft.Extensions.Caching.Memory;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using SmartSure.ClaimsService.Repositories;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Events;
using SmartSure.Shared.Exceptions;

namespace SmartSure.ClaimsService.Services;

public class ClaimAdminService(
    IClaimRepository claimRepository,
    IClaimEventPublisher eventPublisher,
    IMemoryCache memoryCache) : IClaimAdminService
{
    private readonly IClaimRepository _claimRepository = claimRepository;
    private readonly IClaimEventPublisher _eventPublisher = eventPublisher;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<List<ClaimDto>> GetAllClaimsForAdminAsync()
    {
        var claims = await _claimRepository.GetAllAsync();
        return claims.Select(MapClaim).ToList();
    }

    public async Task<ClaimDto> MarkUnderReviewAsync(Guid claimId, Guid adminUserId, string? note)
    {
        return await UpdateClaimStatusByAdminAsync(claimId, adminUserId, ClaimStatus.UnderReview, note);
    }

    public async Task<ClaimDto> ApproveClaimAsync(Guid claimId, Guid adminUserId, decimal? approvedAmount, string? note)
    {
        var claim = await UpdateClaimStatusByAdminAsync(claimId, adminUserId, ClaimStatus.Approved, note);

        await _eventPublisher.PublishClaimApprovedAsync(new ClaimApprovedEvent
        {
            ClaimId = claim.ClaimId,
            UserId = claim.UserId,
            ApprovedAmount = approvedAmount ?? claim.ClaimAmount,
            Note = note,
            ApprovedAtUtc = DateTime.UtcNow
        });

        return claim;
    }

    public async Task<ClaimDto> RejectClaimAsync(Guid claimId, Guid adminUserId, string reason)
    {
        var claim = await UpdateClaimStatusByAdminAsync(claimId, adminUserId, ClaimStatus.Rejected, reason);

        await _eventPublisher.PublishClaimRejectedAsync(new ClaimRejectedEvent
        {
            ClaimId = claim.ClaimId,
            UserId = claim.UserId,
            Reason = reason,
            RejectedAtUtc = DateTime.UtcNow
        });

        return claim;
    }

    private async Task<ClaimDto> UpdateClaimStatusByAdminAsync(Guid claimId, Guid adminUserId, string newStatus, string? note)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId)
                    ?? throw new NotFoundException("Claim not found.");

        var oldStatus = claim.Status;
        if (oldStatus == newStatus)
        {
            throw new ValidationException("Claim status is already set to this value.");
        }

        claim.Status = newStatus;
        claim.AdminNote = note;
        claim.ReviewedBy = adminUserId;
        claim.ReviewedAt = DateTime.UtcNow;
        claim.UpdatedAt = DateTime.UtcNow;

        await _claimRepository.AddStatusHistoryAsync(new ClaimStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            ClaimId = claim.ClaimId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = adminUserId,
            ChangedDate = DateTime.UtcNow,
            Note = note
        });

        await _claimRepository.SaveChangesAsync();
        InvalidateClaimCache(claim.ClaimId, claim.UserId);

        await _eventPublisher.PublishClaimStatusChangedAsync(new ClaimStatusChangedEvent
        {
            ClaimId = claim.ClaimId,
            UserId = adminUserId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            AdminNote = note,
            ChangedAtUtc = DateTime.UtcNow
        });

        return MapClaim(claim);
    }

    private void InvalidateClaimCache(Guid claimId, Guid userId)
    {
        _memoryCache.Remove(GetClaimCacheKey(claimId));
        _memoryCache.Remove(GetUserClaimsCacheKey(userId));
    }

    private static string GetClaimCacheKey(Guid claimId)
    {
        return $"claim_{claimId}";
    }

    private static string GetUserClaimsCacheKey(Guid userId)
    {
        return $"user_claims_{userId}";
    }

    private static ClaimDto MapClaim(Claim claim)
    {
        return new ClaimDto
        {
            ClaimId = claim.ClaimId,
            ClaimNumber = claim.ClaimNumber,
            PolicyId = claim.PolicyId,
            UserId = claim.UserId,
            Description = claim.Description,
            ClaimAmount = claim.ClaimAmount,
            Status = claim.Status,
            AdminNote = claim.AdminNote,
            ReviewedBy = claim.ReviewedBy,
            CreatedDate = claim.CreatedDate,
            SubmittedAt = claim.SubmittedAt,
            ReviewedAt = claim.ReviewedAt,
            UpdatedAt = claim.UpdatedAt
        };
    }
}
