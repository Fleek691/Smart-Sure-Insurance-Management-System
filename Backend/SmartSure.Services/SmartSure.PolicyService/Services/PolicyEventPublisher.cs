using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.PolicyService.Services;

/// <summary>
/// Publishes policy domain events to the RabbitMQ message bus via MassTransit.
/// All other services (Admin, Identity) subscribe to these events for audit and notifications.
/// </summary>
public class PolicyEventPublisher(ILogger<PolicyEventPublisher> logger, IPublishEndpoint publishEndpoint) : IPolicyEventPublisher
{
    private readonly ILogger<PolicyEventPublisher> _logger = logger;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    /// <summary>Published when a policy is activated after successful payment.</summary>
    public async Task PublishActivatedAsync(PolicyActivatedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("PolicyActivated event published for {PolicyNumber}", eventMessage.PolicyNumber);
    }

    /// <summary>Published when a policy is cancelled by the customer or admin.</summary>
    public async Task PublishCancelledAsync(PolicyCancelledEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("PolicyCancelled event published for {PolicyNumber}", eventMessage.PolicyNumber);
    }
}