namespace SmartSure.ClaimsService.Models;

public class ClaimStatusHistory
{
    public Guid HistoryId { get; set; }
    public Guid ClaimId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; }
    public string? Note { get; set; }

    public Claim? Claim { get; set; }
}