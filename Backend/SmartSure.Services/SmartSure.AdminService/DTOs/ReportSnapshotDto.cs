namespace SmartSure.AdminService.DTOs;

/// <summary>
/// Lightweight DTO used internally to carry a persisted report between the repository,
/// memory cache, and the PDF builder. Mirrors the <see cref="Models.Report"/> entity.
/// </summary>
public class ReportSnapshotDto
{
    public Guid ReportId { get; set; }

    /// <summary>Report category — "POLICY", "CLAIM", or "REVENUE".</summary>
    public string ReportType { get; set; } = string.Empty;

    public DateTime GeneratedDate { get; set; }
    public Guid UserId { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }

    /// <summary>JSON-serialized report DTO — deserialized by the PDF builder to render content.</summary>
    public string DataJson { get; set; } = string.Empty;
}
