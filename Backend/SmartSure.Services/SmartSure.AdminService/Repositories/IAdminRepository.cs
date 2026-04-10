using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories;

public interface IAdminRepository
{
    Task<int> CountDistinctEntitiesAsync(string entityType, DateTime? fromUtc, DateTime? toUtc);
    Task<decimal> SumAmountFromAuditDetailsAsync(string entityType, DateTime? fromUtc, DateTime? toUtc);
    Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? action, string? entityType, int page, int pageSize);
    Task<int> GetAuditLogsCountAsync(DateTime? fromUtc, DateTime? toUtc, string? action, string? entityType);
    Task<List<AuditLog>> GetPolicyLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? typeFilter);
    Task<List<AuditLog>> GetClaimLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? statusFilter);
    Task<List<AuditLog>> GetRevenueLogsAsync(DateTime? fromUtc, DateTime? toUtc);
    Task AddReportAsync(Report report);
    Task<Report?> GetReportByIdAsync(Guid reportId);
    Task SaveChangesAsync();
}
