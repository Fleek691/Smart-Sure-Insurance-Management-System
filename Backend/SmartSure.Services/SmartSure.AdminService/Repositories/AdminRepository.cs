using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories;

public class AdminRepository(AdminDbContext dbContext) : IAdminRepository
{
    private readonly AdminDbContext _dbContext = dbContext;

    public async Task<int> CountDistinctEntitiesAsync(string entityType, DateTime? fromUtc, DateTime? toUtc)
    {
        var query = BuildAuditQuery(fromUtc, toUtc, null, entityType);
        return await query.Select(x => x.EntityId).Distinct().CountAsync();
    }

    public async Task<decimal> SumAmountFromAuditDetailsAsync(string entityType, DateTime? fromUtc, DateTime? toUtc)
    {
        var logs = await BuildAuditQuery(fromUtc, toUtc, null, entityType).AsNoTracking().ToListAsync();
        decimal total = 0m;

        foreach (var log in logs)
        {
            total += ExtractAmount(log.Details);
        }

        return total;
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? action, string? entityType, int page, int pageSize)
    {
        return await BuildAuditQuery(fromUtc, toUtc, action, entityType)
            .OrderByDescending(x => x.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetAuditLogsCountAsync(DateTime? fromUtc, DateTime? toUtc, string? action, string? entityType)
    {
        return await BuildAuditQuery(fromUtc, toUtc, action, entityType).CountAsync();
    }

    public async Task<List<AuditLog>> GetPolicyLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? typeFilter)
    {
        var query = BuildAuditQuery(fromUtc, toUtc, null, "Policy");

        if (!string.IsNullOrWhiteSpace(typeFilter))
        {
            query = query.Where(x => x.Action.Contains(typeFilter));
        }

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<List<AuditLog>> GetClaimLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? statusFilter)
    {
        var query = BuildAuditQuery(fromUtc, toUtc, null, "Claim");

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(x => x.Action.Contains(statusFilter));
        }

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<List<AuditLog>> GetRevenueLogsAsync(DateTime? fromUtc, DateTime? toUtc)
    {
        return await BuildAuditQuery(fromUtc, toUtc, null, "Policy")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddReportAsync(Report report)
    {
        await _dbContext.Reports.AddAsync(report);
    }

    public async Task<Report?> GetReportByIdAsync(Guid reportId)
    {
        return await _dbContext.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ReportId == reportId);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<AuditLog> BuildAuditQuery(DateTime? fromUtc, DateTime? toUtc, string? action, string? entityType)
    {
        var query = _dbContext.AuditLogs.AsQueryable();

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.TimeStamp >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.TimeStamp <= toUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.Action.Contains(action));
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(x => x.EntityType == entityType);
        }

        return query;
    }

    private static decimal ExtractAmount(string? detailsJson)
    {
        if (string.IsNullOrWhiteSpace(detailsJson))
        {
            return 0m;
        }

        try
        {
            using var doc = JsonDocument.Parse(detailsJson);
            if (TryGetDecimal(doc.RootElement, "amount", out var amount))
            {
                return amount;
            }

            if (TryGetDecimal(doc.RootElement, "monthlyPremium", out var monthlyPremium))
            {
                return monthlyPremium;
            }

            if (TryGetDecimal(doc.RootElement, "premium", out var premium))
            {
                return premium;
            }

            if (TryGetDecimal(doc.RootElement, "approvedAmount", out var approvedAmount))
            {
                return approvedAmount;
            }

            return 0m;
        }
        catch (JsonException)
        {
            return 0m;
        }
    }

    private static bool TryGetDecimal(JsonElement element, string propertyName, out decimal value)
    {
        value = 0m;
        foreach (var property in element.EnumerateObject())
        {
            if (!property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetDecimal(out var decimalValue))
            {
                value = decimalValue;
                return true;
            }

            if (property.Value.ValueKind == JsonValueKind.String && decimal.TryParse(property.Value.GetString(), out var parsed))
            {
                value = parsed;
                return true;
            }
        }

        return false;
    }
}
