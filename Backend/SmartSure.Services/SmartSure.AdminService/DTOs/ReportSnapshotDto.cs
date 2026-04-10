namespace SmartSure.AdminService.DTOs;

public class ReportSnapshotDto
{
    public Guid ReportId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public Guid UserId { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
    public string DataJson { get; set; } = string.Empty;
}
