using SmartSure.AdminService.DTOs;

namespace SmartSure.AdminService.Services;

public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<PolicyReportDto> GetPolicyReportAsync(Guid adminUserId, DateOnly? from, DateOnly? to, string? typeFilter);
    Task<ClaimsReportDto> GetClaimsReportAsync(Guid adminUserId, DateOnly? from, DateOnly? to, string? statusFilter);
    Task<RevenueReportDto> GetRevenueReportAsync(Guid adminUserId, DateOnly? from, DateOnly? to);
    Task<(string FileName, byte[] PdfContent)> ExportReportPdfAsync(Guid reportId);
    Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(DateOnly? from, DateOnly? to, string? action, string? entityType, int page, int pageSize);
}
