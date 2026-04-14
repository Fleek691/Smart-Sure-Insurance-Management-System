using SmartSure.Shared.Events;

namespace SmartSure.ClaimsService.Services;

/// <summary>
/// Publishes claim lifecycle events to the message bus.
/// Consumers in AdminService and IdentityService subscribe to these events.
/// </summary>
public interface IClaimEventPublisher
{
    /// <summary>Fired when a customer submits a Draft claim for admin review.</summary>
    Task PublishClaimSubmittedAsync(ClaimSubmittedEvent eventMessage);

    /// <summary>Fired on every admin status transition.</summary>
    Task PublishClaimStatusChangedAsync(ClaimStatusChangedEvent eventMessage);

    /// <summary>Fired when an admin approves a claim.</summary>
    Task PublishClaimApprovedAsync(ClaimApprovedEvent eventMessage);

    /// <summary>Fired when an admin rejects a claim.</summary>
    Task PublishClaimRejectedAsync(ClaimRejectedEvent eventMessage);
}