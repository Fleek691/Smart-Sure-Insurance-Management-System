using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.PolicyService.Services;

public class PolicyEventPublisher(ILogger<PolicyEventPublisher> logger, IPublishEndpoint publishEndpoint) : IPolicyEventPublisher
{
    private readonly ILogger<PolicyEventPublisher> _logger = logger;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task PublishActivatedAsync(PolicyActivatedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("PolicyActivated event published for {PolicyNumber}", eventMessage.PolicyNumber);
    }

    public async Task PublishCancelledAsync(PolicyCancelledEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("PolicyCancelled event published for {PolicyNumber}", eventMessage.PolicyNumber);
    }
}