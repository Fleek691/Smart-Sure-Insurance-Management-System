namespace SmartSure.AdminService.DTOs;

public class ClaimsReportDto
{
    public Guid ReportId { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
    public string? StatusFilter { get; set; }
    public int TotalClaims { get; set; }
    public List<ClaimsReportItemDto> Items { get; set; } = new();
}

public class ClaimsReportItemDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
