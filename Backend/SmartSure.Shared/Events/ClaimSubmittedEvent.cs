namespace SmartSure.Shared.Events;

public class ClaimSubmittedEvent
{
    public Guid ClaimId { get; set; }
    public Guid PolicyId { get; set; }
    public Guid UserId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
}
