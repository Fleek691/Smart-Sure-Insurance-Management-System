using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Consumers;

/// <summary>
/// Listens for <see cref="PolicyCancelledEvent"/> messages from the PolicyService.
/// Currently logs the event. Extend to send cancellation notifications or update user state.
/// </summary>
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
