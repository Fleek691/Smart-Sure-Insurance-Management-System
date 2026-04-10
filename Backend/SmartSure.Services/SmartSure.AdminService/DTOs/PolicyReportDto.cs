namespace SmartSure.AdminService.DTOs;

public class PolicyReportDto
{
    public Guid ReportId { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
    public string? TypeFilter { get; set; }
    public int TotalPolicies { get; set; }
    public List<PolicyReportItemDto> Items { get; set; } = new();
}

public class PolicyReportItemDto
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime LastActionAt { get; set; }
}
