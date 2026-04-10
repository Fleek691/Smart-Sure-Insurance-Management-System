using System.Security.Claims;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Exceptions;

namespace SmartSure.AdminService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController(IAdminService adminService) : ControllerBase
{
    private readonly IAdminService _adminService = adminService;

    [HttpGet("dashboard/stats")]
    public async Task<DashboardStatsDto> GetDashboardStats()
    {
        return await _adminService.GetDashboardStatsAsync();
    }

    [HttpGet("reports/policies")]
    public async Task<PolicyReportDto> GetPolicyReport([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? type)
    {
        return await _adminService.GetPolicyReportAsync(GetUserId(), from, to, type);
    }

    [HttpGet("reports/claims")]
    public async Task<ClaimsReportDto> GetClaimsReport([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? status)
    {
        return await _adminService.GetClaimsReportAsync(GetUserId(), from, to, status);
    }

    [HttpGet("reports/revenue")]
    public async Task<RevenueReportDto> GetRevenueReport([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        return await _adminService.GetRevenueReportAsync(GetUserId(), from, to);
    }

    [HttpGet("reports/export")]
    public async Task<IActionResult> ExportReport([FromQuery] Guid reportId)
    {
        var (fileName, pdfContent) = await _adminService.ExportReportPdfAsync(reportId);
        Response.Headers.CacheControl = "no-store";
        Response.Headers["Content-Disposition"] = new ContentDisposition
        {
            FileName = fileName,
            Inline = false
        }.ToString();
        return File(pdfContent, "application/pdf", fileName);
    }

    [HttpGet("audit-logs")]
    public async Task<PagedResultDto<AuditLogDto>> GetAuditLogs(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _adminService.GetAuditLogsAsync(from, to, action, entityType, page, pageSize);
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
