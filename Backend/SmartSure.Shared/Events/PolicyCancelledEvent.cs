namespace SmartSure.Shared.Events;

public class PolicyCancelledEvent
{
    public Guid PolicyId { get; set; }
    public Guid UserId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CancelledAtUtc { get; set; } = DateTime.UtcNow;
}
