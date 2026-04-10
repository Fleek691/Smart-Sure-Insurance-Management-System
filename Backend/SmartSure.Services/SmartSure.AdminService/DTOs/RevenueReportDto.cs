namespace SmartSure.AdminService.DTOs;

public class RevenueReportDto
{
    public Guid ReportId { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RevenuePointDto> Points { get; set; } = new();
}

public class RevenuePointDto
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
}
