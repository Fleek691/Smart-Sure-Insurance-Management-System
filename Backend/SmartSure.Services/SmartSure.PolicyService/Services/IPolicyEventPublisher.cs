using SmartSure.Shared.Events;

namespace SmartSure.PolicyService.Services;

/// <summary>
/// Publishes policy lifecycle events to the message bus.
/// Consumers in AdminService and IdentityService subscribe to these events.
/// </summary>
public interface IPolicyEventPublisher
{
    /// <summary>Fired when a policy is activated (purchased and paid for).</summary>
    Task PublishActivatedAsync(PolicyActivatedEvent eventMessage);

    /// <summary>Fired when a policy is cancelled by the customer or admin.</summary>
    Task PublishCancelledAsync(PolicyCancelledEvent eventMessage);
}