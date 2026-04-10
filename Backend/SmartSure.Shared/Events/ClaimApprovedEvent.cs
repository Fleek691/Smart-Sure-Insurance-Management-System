namespace SmartSure.Shared.Events;

public class ClaimApprovedEvent
{
    public Guid ClaimId { get; set; }
    public Guid UserId { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string? Note { get; set; }
    public DateTime ApprovedAtUtc { get; set; } = DateTime.UtcNow;
}
