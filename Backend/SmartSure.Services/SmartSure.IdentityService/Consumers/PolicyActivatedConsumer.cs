using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Consumers;

public class PolicyActivatedConsumer(ILogger<PolicyActivatedConsumer> logger) : IConsumer<PolicyActivatedEvent>
{
    private readonly ILogger<PolicyActivatedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<PolicyActivatedEvent> context)
    {
        _logger.LogInformation("PolicyActivated consumed for PolicyId {PolicyId} and UserId {UserId}", context.Message.PolicyId, context.Message.UserId);
        return Task.CompletedTask;
    }
}
