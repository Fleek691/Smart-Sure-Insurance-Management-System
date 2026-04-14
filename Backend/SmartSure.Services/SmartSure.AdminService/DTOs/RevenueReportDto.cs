namespace SmartSure.AdminService.DTOs;

/// <summary>
/// Revenue report DTO with daily data points for the requested period.
/// Persisted as a snapshot and exportable as a PDF.
/// </summary>
public class RevenueReportDto
{
    /// <summary>Assigned after the report is persisted — used to request a PDF export.</summary>
    public Guid ReportId { get; set; }

    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }

    /// <summary>Sum of all daily amounts in the period.</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>Daily revenue data points ordered by date ascending.</summary>
    public List<RevenuePointDto> Points { get; set; } = new();
}

/// <summary>A single daily revenue data point.</summary>
public class RevenuePointDto
{
    public DateOnly Date { get; set; }

    /// <summary>Total revenue extracted from policy audit log entries on this date.</summary>
    public decimal Amount { get; set; }
}
