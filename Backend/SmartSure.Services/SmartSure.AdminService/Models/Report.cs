namespace SmartSure.AdminService.Models;

/// <summary>
/// Persisted snapshot of a generated report.
/// Stored so the admin can export the same report as a PDF later without regenerating it.
/// </summary>
public class Report
{
    public Guid ReportId { get; set; }

    /// <summary>Report category — "POLICY", "CLAIM", or "REVENUE".</summary>
    public string ReportType { get; set; } = string.Empty;

    public DateTime GeneratedDate { get; set; }

    /// <summary>Admin user who generated the report.</summary>
    public Guid UserId { get; set; }

    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }

    /// <summary>JSON-serialized report DTO — deserialized at export time to build the PDF.</summary>
    public string DataJson { get; set; } = string.Empty;
}
