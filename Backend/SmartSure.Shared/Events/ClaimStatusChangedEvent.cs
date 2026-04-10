namespace SmartSure.Shared.Events;

public class ClaimStatusChangedEvent
{
    public Guid ClaimId { get; set; }
    public Guid UserId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
