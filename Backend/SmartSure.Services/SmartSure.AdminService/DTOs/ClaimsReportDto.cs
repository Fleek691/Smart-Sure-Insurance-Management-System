namespace SmartSure.AdminService.DTOs;

/// <summary>
/// Report DTO for claims activity grouped by status.
/// Persisted as a snapshot and exportable as a PDF.
/// </summary>
public class ClaimsReportDto
{
    /// <summary>Assigned after the report is persisted — used to request a PDF export.</summary>
    public Guid ReportId { get; set; }

    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }

    /// <summary>Optional claim status filter applied when generating the report.</summary>
    public string? StatusFilter { get; set; }

    /// <summary>Total distinct claims in the period.</summary>
    public int TotalClaims { get; set; }

    /// <summary>Breakdown of claim actions grouped by status.</summary>
    public List<ClaimsReportItemDto> Items { get; set; } = new();
}

/// <summary>A single row in the claims report — one status with its count.</summary>
public class ClaimsReportItemDto
{
    /// <summary>Claim status or action constant, e.g. "CLAIM_APPROVED".</summary>
    public string Status { get; set; } = string.Empty;

    public int Count { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
