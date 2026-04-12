using Microsoft.Extensions.Caching.Memory;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Models;
using SmartSure.ClaimsService.Repositories;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Events;
using SmartSure.Shared.Exceptions;

namespace SmartSure.ClaimsService.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _claimRepository;
    private readonly IClaimEventPublisher _eventPublisher;
    private readonly IMemoryCache _memoryCache;
    private readonly IMegaStorageService _megaStorageService;

    public ClaimService(
        IClaimRepository claimRepository,
        IClaimEventPublisher eventPublisher,
        IMemoryCache memoryCache,
        IMegaStorageService megaStorageService)
    {
        _claimRepository = claimRepository;
        _eventPublisher = eventPublisher;
        _memoryCache = memoryCache;
        _megaStorageService = megaStorageService;
    }

    public async Task<ClaimDto> CreateClaimAsync(Guid userId, CreateClaimDto dto)
    {
        if (dto.PolicyId == Guid.Empty)
        {
            throw new ValidationException("Please select a policy to file a claim against.");
        }

        if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Trim().Length < 10)
        {
            throw new ValidationException("Please provide a detailed description (at least 10 characters).");
        }

        if (dto.ClaimAmount <= 0)
        {
            throw new ValidationException("Claim amount must be greater than zero.");
        }

        var now = DateTime.UtcNow;
        var claim = new Claim
        {
            ClaimId = Guid.NewGuid(),
            ClaimNumber = $"CLM-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            PolicyId = dto.PolicyId,
            UserId = userId,
            Description = dto.Description.Trim(),
            ClaimAmount = dto.ClaimAmount,
            Status = ClaimStatus.Draft,
            CreatedDate = now,
            UpdatedAt = now
        };

        await _claimRepository.AddAsync(claim);
        await _claimRepository.AddStatusHistoryAsync(new ClaimStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            ClaimId = claim.ClaimId,
            OldStatus = ClaimStatus.Draft,
            NewStatus = ClaimStatus.Draft,
            ChangedBy = userId,
            ChangedDate = now,
            Note = "Claim initiated"
        });
        await _claimRepository.SaveChangesAsync();

        InvalidateClaimCache(claim.ClaimId, claim.UserId);
        return MapClaim(claim);
    }

    public async Task<List<ClaimDto>> GetMyClaimsAsync(Guid userId)
    {
        var cacheKey = GetUserClaimsCacheKey(userId);
        if (_memoryCache.TryGetValue(cacheKey, out List<ClaimDto>? cachedClaims) && cachedClaims is not null)
        {
            return cachedClaims;
        }

        var claims = await _claimRepository.GetByUserIdAsync(userId);
        var result = claims.Select(MapClaim).ToList();
        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
        return result;
    }

    public async Task<ClaimDto> GetClaimAsync(Guid claimId, Guid userId, bool isAdmin)
    {
        var cacheKey = GetClaimCacheKey(claimId);
        if (_memoryCache.TryGetValue(cacheKey, out ClaimDto? cachedClaim) && cachedClaim is not null)
        {
            if (isAdmin || cachedClaim.UserId == userId) return cachedClaim;
            throw new ForbiddenException("You are not allowed to view this claim.");
        }

        var claim = await _claimRepository.GetByIdAsync(claimId)
                    ?? throw new NotFoundException("Claim not found.");

        if (!isAdmin && claim.UserId != userId)
        {
            throw new ForbiddenException("You are not allowed to view this claim.");
        }

        var result = MapClaim(claim);
        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
        return result;
    }

    public async Task<ClaimDto> SubmitClaimAsync(Guid claimId, Guid userId)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId)
                    ?? throw new NotFoundException("Claim not found.");

        if (claim.UserId != userId)
        {
            throw new ForbiddenException("You are not allowed to submit this claim.");
        }

        if (claim.Status != ClaimStatus.Draft)
        {
            throw new BusinessRuleException("Only draft claims can be submitted.");
        }

        var documents = await _claimRepository.GetDocumentsAsync(claimId);
        if (!documents.Any())
        {
            throw new BusinessRuleException("Please upload at least one supporting document before submitting your claim.");
        }

        var oldStatus = claim.Status;
        claim.Status = ClaimStatus.Submitted;
        claim.SubmittedAt = DateTime.UtcNow;
        claim.UpdatedAt = DateTime.UtcNow;

        await _claimRepository.AddStatusHistoryAsync(new ClaimStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            ClaimId = claim.ClaimId,
            OldStatus = oldStatus,
            NewStatus = claim.Status,
            ChangedBy = userId,
            ChangedDate = DateTime.UtcNow,
            Note = "Submitted by customer"
        });

        await _claimRepository.SaveChangesAsync();
        InvalidateClaimCache(claim.ClaimId, claim.UserId);

        await _eventPublisher.PublishClaimSubmittedAsync(new ClaimSubmittedEvent
        {
            ClaimId = claim.ClaimId,
            PolicyId = claim.PolicyId,
            UserId = claim.UserId,
            ClaimNumber = claim.ClaimNumber,
            SubmittedAtUtc = claim.SubmittedAt ?? DateTime.UtcNow
        });

        await _eventPublisher.PublishClaimStatusChangedAsync(new ClaimStatusChangedEvent
        {
            ClaimId = claim.ClaimId,
            UserId = userId,
            OldStatus = oldStatus,
            NewStatus = claim.Status,
            ChangedAtUtc = DateTime.UtcNow
        });

        return MapClaim(claim);
    }

    public async Task<ClaimDocumentDto> UploadDocumentAsync(Guid claimId, Guid userId, UploadClaimDocumentDto dto)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId)
                    ?? throw new NotFoundException("Claim not found.");

        if (claim.UserId != userId)
        {
            throw new ForbiddenException("You are not allowed to upload documents for this claim.");
        }

        if (claim.Status != ClaimStatus.Draft)
        {
            throw new BusinessRuleException("Documents can only be uploaded to draft claims.");
        }

        var ext = dto.FileType.Trim().ToLowerInvariant();
        if (ext != "pdf" && ext != "jpg" && ext != "png")
        {
            throw new ValidationException("Only PDF, JPG, and PNG file types are allowed.");
        }

        if (dto.FileSizeKb <= 0)
        {
            throw new ValidationException("File size must be greater than zero.");
        }

        if (dto.FileSizeKb > 10_240)
        {
            throw new ValidationException("File size cannot exceed 10 MB.");
        }

        if (string.IsNullOrWhiteSpace(dto.FileName))
        {
            throw new ValidationException("File name is required.");
        }

        var content = ParseBase64(dto.ContentBase64);
        var (megaFileId, megaFileUrl) = await _megaStorageService.UploadAsync(dto.FileName.Trim(), content);

        var doc = new ClaimDocument
        {
            DocId = Guid.NewGuid(),
            ClaimId = claimId,
            FileName = dto.FileName.Trim(),
            MegaNzFileId = megaFileId,
            FileUrl = megaFileUrl,
            FileType = ext,
            FileSizeKb = dto.FileSizeKb,
            UploadedAt = DateTime.UtcNow
        };

        await _claimRepository.AddDocumentAsync(doc);
        claim.UpdatedAt = DateTime.UtcNow;
        await _claimRepository.SaveChangesAsync();
        InvalidateClaimCache(claim.ClaimId, claim.UserId);

        return MapDocument(doc);
    }

    public async Task<List<ClaimDocumentDto>> GetDocumentsAsync(Guid claimId, Guid userId, bool isAdmin)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId)
                    ?? throw new NotFoundException("Claim not found.");

        if (!isAdmin && claim.UserId != userId)
        {
            throw new ForbiddenException("You are not allowed to view documents for this claim.");
        }

        var documents = await _claimRepository.GetDocumentsAsync(claimId);
        return documents.Select(MapDocument).ToList();
    }

    public async Task DeleteDocumentAsync(Guid claimId, Guid docId, Guid userId)
    {
        var claim = await _claimRepository.GetByIdAsync(claimId)
                    ?? throw new NotFoundException("Claim not found.");

        if (claim.UserId != userId)
        {
            throw new ForbiddenException("You are not allowed to delete documents for this claim.");
        }

        if (claim.Status != ClaimStatus.Draft)
        {
            throw new BusinessRuleException("Documents can only be deleted from draft claims.");
        }

        var document = await _claimRepository.GetDocumentByIdAsync(claimId, docId)
                       ?? throw new NotFoundException("Document not found.");

        await _claimRepository.RemoveDocumentAsync(document);
        claim.UpdatedAt = DateTime.UtcNow;
        await _claimRepository.SaveChangesAsync();
        InvalidateClaimCache(claim.ClaimId, claim.UserId);
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

    private static ClaimDocumentDto MapDocument(ClaimDocument document)
    {
        return new ClaimDocumentDto
        {
            DocId = document.DocId,
            ClaimId = document.ClaimId,
            FileName = document.FileName,
            MegaNzFileId = document.MegaNzFileId,
            FileUrl = document.FileUrl,
            FileType = document.FileType,
            FileSizeKb = document.FileSizeKb,
            UploadedAt = document.UploadedAt
        };
    }

    private static byte[] ParseBase64(string input)
    {
        var value = input.Trim();
        var markerIndex = value.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
        if (markerIndex >= 0)
        {
            value = value[(markerIndex + "base64,".Length)..];
        }

        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            throw new ValidationException("Invalid base64 content for document upload.");
        }
    }
}
