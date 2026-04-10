namespace SmartSure.Shared.Events;

public class PolicyActivatedEvent
{
    public Guid PolicyId { get; set; }
    public Guid UserId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime ActivatedAtUtc { get; set; } = DateTime.UtcNow;
}
