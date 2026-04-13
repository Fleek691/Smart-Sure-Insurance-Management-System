using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.ClaimsService.DTOs;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Exceptions;

namespace SmartSure.ClaimsService.Controllers;

[ApiController]
[Route("api/claims")]
[Authorize]
public class ClaimsController(IClaimService claimService, IClaimAdminService claimAdminService) : ControllerBase
{
    private readonly IClaimService _claimService = claimService;
    private readonly IClaimAdminService _claimAdminService = claimAdminService;

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ClaimDto> Create([FromBody] CreateClaimDto dto)
    {
        var userId = GetUserId();
        var bearerToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "").Trim();
        return await _claimService.CreateClaimAsync(userId, dto, bearerToken);
    }

    [HttpGet("my-claims")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<List<ClaimDto>> GetMyClaims()
    {
        var userId = GetUserId();
        return await _claimService.GetMyClaimsAsync(userId);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.Customer},{Roles.Admin}")]
    public async Task<ClaimDto> GetById(Guid id)
    {
        var userId  = GetUserId();
        var isAdmin = User.IsInRole(Roles.Admin);
        return await _claimService.GetClaimAsync(id, userId, isAdmin);
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ClaimDto> Submit(Guid id)
    {
        var userId = GetUserId();
        return await _claimService.SubmitClaimAsync(id, userId);
    }

    [HttpPost("{id:guid}/documents")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ClaimDocumentDto> UploadDocument(Guid id, [FromBody] UploadClaimDocumentDto dto)
    {
        var userId = GetUserId();
        return await _claimService.UploadDocumentAsync(id, userId, dto);
    }

    [HttpGet("{id:guid}/documents")]
    [Authorize(Roles = $"{Roles.Customer},{Roles.Admin}")]
    public async Task<List<ClaimDocumentDto>> GetDocuments(Guid id)
    {
        var userId  = GetUserId();
        var isAdmin = User.IsInRole(Roles.Admin);
        return await _claimService.GetDocumentsAsync(id, userId, isAdmin);
    }

    [HttpDelete("{id:guid}/documents/{docId:guid}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid docId)
    {
        var userId = GetUserId();
        await _claimService.DeleteDocumentAsync(id, docId, userId);
        return NoContent();
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<List<ClaimDto>> GetAllForAdmin()
    {
        return await _claimAdminService.GetAllClaimsForAdminAsync();
    }

    [HttpPut("admin/{id:guid}/review")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ClaimDto> SetUnderReview(Guid id, [FromBody] ReviewClaimDto dto)
    {
        var adminId = GetUserId();
        return await _claimAdminService.MarkUnderReviewAsync(id, adminId, dto.Note);
    }

    [HttpPut("admin/{id:guid}/approve")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ClaimDto> Approve(Guid id, [FromBody] ApproveClaimDto dto)
    {
        var adminId = GetUserId();
        return await _claimAdminService.ApproveClaimAsync(id, adminId, dto.ApprovedAmount, dto.Note);
    }

    [HttpPut("admin/{id:guid}/reject")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ClaimDto> Reject(Guid id, [FromBody] RejectClaimDto dto)
    {
        var adminId = GetUserId();
        return await _claimAdminService.RejectClaimAsync(id, adminId, dto.Reason);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new UnauthorizedException("User id claim not found.");
    }
}