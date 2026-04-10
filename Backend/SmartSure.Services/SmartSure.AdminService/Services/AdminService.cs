using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using SmartSure.AdminService.DTOs;
using SmartSure.AdminService.Models;
using SmartSure.AdminService.Repositories;
using SmartSure.Shared.Exceptions;

namespace SmartSure.AdminService.Services;

public class AdminService(IAdminRepository adminRepository, IMemoryCache memoryCache) : IAdminService
{
    private const string DashboardStatsCacheKey = "admin_dashboard_stats";

    private readonly IAdminRepository _adminRepository = adminRepository;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        if (_memoryCache.TryGetValue(DashboardStatsCacheKey, out DashboardStatsDto? cachedStats) && cachedStats is not null)
        {
            return cachedStats;
        }

        var now = DateTime.UtcNow;
        var fromUtc = now.AddDays(-365);

        var stats = new DashboardStatsDto
        {
            TotalPolicies = await _adminRepository.CountDistinctEntitiesAsync("Policy", fromUtc, now),
            TotalClaims = await _adminRepository.CountDistinctEntitiesAsync("Claim", fromUtc, now),
            TotalRevenue = Math.Round(await _adminRepository.SumAmountFromAuditDetailsAsync("Policy", fromUtc, now), 2)
        };

        _memoryCache.Set(DashboardStatsCacheKey, stats, TimeSpan.FromMinutes(2));
        return stats;
    }

    public async Task<PolicyReportDto> GetPolicyReportAsync(Guid adminUserId, DateOnly? from, DateOnly? to, string? typeFilter)
    {
        var (fromUtc, toUtc) = BuildUtcRange(from, to);
        var logs = await _adminRepository.GetPolicyLogsAsync(fromUtc, toUtc, typeFilter);

        var items = logs
            .GroupBy(x => x.Action)
            .Select(x => new PolicyReportItemDto
            {
                Action = x.Key,
                Count = x.Count(),
                LastActionAt = x.Max(y => y.TimeStamp)
            })
            .OrderByDescending(x => x.LastActionAt)
            .ToList();

        var dto = new PolicyReportDto
        {
            PeriodFrom = from,
            PeriodTo = to,
            TypeFilter = typeFilter,
            TotalPolicies = logs.Select(x => x.EntityId).Distinct().Count(),
            Items = items
        };

        var report = await SaveReportAsync("POLICY", adminUserId, from, to, dto);
        dto.ReportId = report.ReportId;
        return dto;
    }

    public async Task<ClaimsReportDto> GetClaimsReportAsync(Guid adminUserId, DateOnly? from, DateOnly? to, string? statusFilter)
    {
        var (fromUtc, toUtc) = BuildUtcRange(from, to);
        var logs = await _adminRepository.GetClaimLogsAsync(fromUtc, toUtc, statusFilter);

        var items = logs
            .GroupBy(x => x.Action)
            .Select(x => new ClaimsReportItemDto
            {
                Status = x.Key,
                Count = x.Count(),
                LastUpdatedAt = x.Max(y => y.TimeStamp)
            })
            .OrderByDescending(x => x.LastUpdatedAt)
            .ToList();

        var dto = new ClaimsReportDto
        {
            PeriodFrom = from,
            PeriodTo = to,
            StatusFilter = statusFilter,
            TotalClaims = logs.Select(x => x.EntityId).Distinct().Count(),
            Items = items
        };

        var report = await SaveReportAsync("CLAIM", adminUserId, from, to, dto);
        dto.ReportId = report.ReportId;
        return dto;
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(Guid adminUserId, DateOnly? from, DateOnly? to)
    {
        var (fromUtc, toUtc) = BuildUtcRange(from, to);
        var logs = await _adminRepository.GetRevenueLogsAsync(fromUtc, toUtc);

        var points = logs
            .GroupBy(x => DateOnly.FromDateTime(x.TimeStamp))
            .Select(x => new RevenuePointDto
            {
                Date = x.Key,
                Amount = Math.Round(x.Sum(y => ExtractAmount(y.Details)), 2)
            })
            .OrderBy(x => x.Date)
            .ToList();

        var dto = new RevenueReportDto
        {
            PeriodFrom = from,
            PeriodTo = to,
            TotalRevenue = points.Sum(x => x.Amount),
            Points = points
        };

        var report = await SaveReportAsync("REVENUE", adminUserId, from, to, dto);
        dto.ReportId = report.ReportId;
        return dto;
    }

    public async Task<(string FileName, byte[] PdfContent)> ExportReportPdfAsync(Guid reportId)
    {
        var cacheKey = GetReportCacheKey(reportId);
        if (_memoryCache.TryGetValue(cacheKey, out ReportSnapshotDto? cachedReport) && cachedReport is not null)
        {
            return BuildPdf(cachedReport);
        }

        var report = await _adminRepository.GetReportByIdAsync(reportId)
            ?? throw new NotFoundException("Report not found.");

        var dto = MapReport(report);
        _memoryCache.Set(cacheKey, dto, TimeSpan.FromMinutes(10));
        return BuildPdf(dto);
    }

    public async Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(DateOnly? from, DateOnly? to, string? action, string? entityType, int page, int pageSize)
    {
        if (page <= 0)
        {
            throw new ValidationException("Page must be greater than zero.");
        }

        if (pageSize <= 0 || pageSize > 200)
        {
            throw new ValidationException("Page size must be between 1 and 200.");
        }

        var (fromUtc, toUtc) = BuildUtcRange(from, to);
        var logs = await _adminRepository.GetAuditLogsAsync(fromUtc, toUtc, action, entityType, page, pageSize);
        var total = await _adminRepository.GetAuditLogsCountAsync(fromUtc, toUtc, action, entityType);

        return new PagedResultDto<AuditLogDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = logs.Select(MapAuditLog).ToList()
        };
    }

    private async Task<Report> SaveReportAsync<T>(string reportType, Guid adminUserId, DateOnly? from, DateOnly? to, T data)
    {
        var report = new Report
        {
            ReportId = Guid.NewGuid(),
            ReportType = reportType,
            GeneratedDate = DateTime.UtcNow,
            UserId = adminUserId,
            PeriodFrom = from,
            PeriodTo = to,
            DataJson = JsonSerializer.Serialize(data)
        };

        await _adminRepository.AddReportAsync(report);
        await _adminRepository.SaveChangesAsync();

        _memoryCache.Set(GetReportCacheKey(report.ReportId), MapReport(report), TimeSpan.FromMinutes(10));
        return report;
    }

    private static AuditLogDto MapAuditLog(AuditLog log)
    {
        return new AuditLogDto
        {
            LogId = log.LogId,
            UserId = log.UserId,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Details = log.Details,
            TimeStamp = log.TimeStamp
        };
    }

    private static ReportSnapshotDto MapReport(Report report)
    {
        return new ReportSnapshotDto
        {
            ReportId = report.ReportId,
            ReportType = report.ReportType,
            GeneratedDate = report.GeneratedDate,
            UserId = report.UserId,
            PeriodFrom = report.PeriodFrom,
            PeriodTo = report.PeriodTo,
            DataJson = report.DataJson
        };
    }

    private static (DateTime? FromUtc, DateTime? ToUtc) BuildUtcRange(DateOnly? from, DateOnly? to)
    {
        DateTime? fromUtc = from?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        DateTime? toUtc = to?.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return (fromUtc, toUtc);
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

    private static (string FileName, byte[] PdfContent) BuildPdf(ReportSnapshotDto report)
    {
        var fileName = $"{report.ReportType.ToLowerInvariant()}-{report.ReportId}.pdf";
        var lines = new List<string>
        {
            "SmartSure Report",
            $"Report Type: {report.ReportType}",
            $"Report Id: {report.ReportId}",
            $"Generated At (UTC): {report.GeneratedDate:O}",
            $"Period: {report.PeriodFrom:yyyy-MM-dd} to {report.PeriodTo:yyyy-MM-dd}",
            string.Empty
        };

        if (report.ReportType.Equals("POLICY", StringComparison.OrdinalIgnoreCase))
        {
            var payload = JsonSerializer.Deserialize<PolicyReportDto>(report.DataJson) ?? new PolicyReportDto();
            lines.Add($"Total Policies: {payload.TotalPolicies}");
            lines.Add("Action | Count | LastActionAt");
            foreach (var item in payload.Items)
            {
                lines.Add($"{item.Action} | {item.Count} | {item.LastActionAt:O}");
            }
        }
        else if (report.ReportType.Equals("CLAIM", StringComparison.OrdinalIgnoreCase))
        {
            var payload = JsonSerializer.Deserialize<ClaimsReportDto>(report.DataJson) ?? new ClaimsReportDto();
            lines.Add($"Total Claims: {payload.TotalClaims}");
            lines.Add("Status | Count | LastUpdatedAt");
            foreach (var item in payload.Items)
            {
                lines.Add($"{item.Status} | {item.Count} | {item.LastUpdatedAt:O}");
            }
        }
        else if (report.ReportType.Equals("REVENUE", StringComparison.OrdinalIgnoreCase))
        {
            var payload = JsonSerializer.Deserialize<RevenueReportDto>(report.DataJson) ?? new RevenueReportDto();
            lines.Add($"Total Revenue: {payload.TotalRevenue:0.00}");
            lines.Add("Date | Amount");
            foreach (var point in payload.Points)
            {
                lines.Add($"{point.Date:yyyy-MM-dd} | {point.Amount:0.00}");
            }
        }
        else
        {
            lines.Add("DataJson:");
            lines.Add(report.DataJson);
        }

        return (fileName, CreateSimplePdf(lines));
    }

    private static byte[] CreateSimplePdf(List<string> inputLines)
    {
        var lines = inputLines
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Take(44)
            .ToList();

        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 11 Tf");
        contentBuilder.AppendLine("50 770 Td");

        for (var i = 0; i < lines.Count; i++)
        {
            var text = EscapePdfText(lines[i]);
            contentBuilder.AppendLine($"({text}) Tj");
            if (i < lines.Count - 1)
            {
                contentBuilder.AppendLine("0 -16 Td");
            }
        }

        contentBuilder.AppendLine("ET");

        var contentStream = contentBuilder.ToString();
        var objects = new List<string>
        {
            "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
            "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n",
            "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n",
            "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n",
            $"5 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(contentStream)} >>\nstream\n{contentStream}endstream\nendobj\n"
        };

        var pdf = new StringBuilder();
        pdf.Append("%PDF-1.4\n");

        var offsets = new List<int> { 0 };
        var currentLength = Encoding.ASCII.GetByteCount(pdf.ToString());
        foreach (var obj in objects)
        {
            offsets.Add(currentLength);
            pdf.Append(obj);
            currentLength += Encoding.ASCII.GetByteCount(obj);
        }

        var xrefOffset = currentLength;
        pdf.Append($"xref\n0 {objects.Count + 1}\n");
        pdf.Append("0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++)
        {
            pdf.Append($"{offsets[i]:D10} 00000 n \n");
        }

        pdf.Append("trailer\n");
        pdf.Append($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
        pdf.Append("startxref\n");
        pdf.Append($"{xrefOffset}\n");
        pdf.Append("%%EOF");

        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }

    private static string GetReportCacheKey(Guid reportId)
    {
        return $"report_{reportId}";
    }
}
