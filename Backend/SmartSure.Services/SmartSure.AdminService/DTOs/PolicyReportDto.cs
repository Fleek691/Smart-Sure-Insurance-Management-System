namespace SmartSure.AdminService.DTOs;

/// <summary>
/// Report DTO for policy activity grouped by action type.
/// Persisted as a snapshot and exportable as a PDF.
/// </summary>
public class PolicyReportDto
{
    /// <summary>Assigned after the report is persisted — used to request a PDF export.</summary>
    public Guid ReportId { get; set; }

    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }

    /// <summary>Optional insurance type filter applied when generating the report.</summary>
    public string? TypeFilter { get; set; }

    /// <summary>Total distinct policies in the period.</summary>
    public int TotalPolicies { get; set; }

    /// <summary>Breakdown of policy actions (e.g. POLICY_ACTIVATED, POLICY_CANCELLED).</summary>
    public List<PolicyReportItemDto> Items { get; set; } = new();
}

/// <summary>A single row in the policy report — one action type with its count.</summary>
public class PolicyReportItemDto
{
    /// <summary>Action constant, e.g. "POLICY_ACTIVATED".</summary>
    public string Action { get; set; } = string.Empty;

    public int Count { get; set; }
    public DateTime LastActionAt { get; set; }
}
