using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Consumers;

public class PolicyCancelledConsumer(ILogger<PolicyCancelledConsumer> logger) : IConsumer<PolicyCancelledEvent>
{
    private readonly ILogger<PolicyCancelledConsumer> _logger = logger;

    public Task Consume(ConsumeContext<PolicyCancelledEvent> context)
    {
        _logger.LogInformation(
            "PolicyCancelled consumed for PolicyId {PolicyId}, UserId {UserId}, Reason {Reason}",
            context.Message.PolicyId,
            context.Message.UserId,
            context.Message.Reason);

        return Task.CompletedTask;
    }
}
