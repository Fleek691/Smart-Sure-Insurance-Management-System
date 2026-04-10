namespace SmartSure.Shared.Events;

public class ClaimRejectedEvent
{
    public Guid ClaimId { get; set; }
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RejectedAtUtc { get; set; } = DateTime.UtcNow;
}
