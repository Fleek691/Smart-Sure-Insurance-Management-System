using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAdminRepository"/>.
/// All queries are built through a shared <see cref="BuildAuditQuery"/> helper
/// that applies optional date, action, and entity-type filters.
/// </summary>
public class AdminRepository(AdminDbContext dbContext) : IAdminRepository
{
    private readonly AdminDbContext _dbContext = dbContext;

    /// <summary>
    /// Counts distinct entity IDs of the given type within the date range.
    /// Used to populate TotalPolicies and TotalClaims on the dashboard.
    /// </summary>
    public async Task<int> CountDistinctEntitiesAsync(string entityType, DateTime? fromUtc, DateTime? toUtc)
    {
        var query = BuildAuditQuery(fromUtc, toUtc, null, entityType);
        return await query.Select(x => x.EntityId).Distinct().CountAsync();
    }

    /// <summary>
    /// Sums monetary amounts extracted from the JSON Details field of audit logs
    /// for the given entity type and date range. Used for the revenue dashboard KPI.
    /// </summary>
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

    /// <summary>Returns a paginated, ordered page of audit logs matching the given filters.</summary>
    public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? action, string? entityType, int page, int pageSize)
    {
        return await BuildAuditQuery(fromUtc, toUtc, action, entityType)
            .OrderByDescending(x => x.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>Returns the total count of audit logs matching the given filters (for pagination metadata).</summary>
    public async Task<int> GetAuditLogsCountAsync(DateTime? fromUtc, DateTime? toUtc, string? action, string? entityType)
    {
        return await BuildAuditQuery(fromUtc, toUtc, action, entityType).CountAsync();
    }

    /// <summary>Returns policy audit logs, optionally filtered by action substring (e.g. type name).</summary>
    public async Task<List<AuditLog>> GetPolicyLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? typeFilter)
    {
        var query = BuildAuditQuery(fromUtc, toUtc, null, "Policy");

        if (!string.IsNullOrWhiteSpace(typeFilter))
        {
            query = query.Where(x => x.Action.Contains(typeFilter));
        }

        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>Returns claim audit logs, optionally filtered by action substring (e.g. status name).</summary>
    public async Task<List<AuditLog>> GetClaimLogsAsync(DateTime? fromUtc, DateTime? toUtc, string? statusFilter)
    {
        var query = BuildAuditQuery(fromUtc, toUtc, null, "Claim");

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(x => x.Action.Contains(statusFilter));
        }

        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>Returns all policy audit logs in the date range — used to build the revenue report.</summary>
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

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds a base IQueryable with optional date range, action, and entity-type filters.
    /// All public query methods compose on top of this.
    /// </summary>
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

    /// <summary>
    /// Tries to extract a monetary amount from an audit log's JSON Details field.
    /// Checks "amount", "monthlyPremium", "premium", and "approvedAmount" in order.
    /// Returns 0 if the field is missing or the JSON is malformed.
    /// </summary>
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

    /// <summary>
    /// Case-insensitive property lookup on a JSON element.
    /// Handles both numeric and string-encoded decimal values.
    /// </summary>
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
