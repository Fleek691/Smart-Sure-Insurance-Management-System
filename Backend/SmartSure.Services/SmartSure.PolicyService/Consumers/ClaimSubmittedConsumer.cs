using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.PolicyService.Consumers;

public class ClaimSubmittedConsumer(ILogger<ClaimSubmittedConsumer> logger) : IConsumer<ClaimSubmittedEvent>
{
    private readonly ILogger<ClaimSubmittedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<ClaimSubmittedEvent> context)
    {
        _logger.LogInformation(
            "ClaimSubmitted consumed in PolicyService for ClaimId {ClaimId}, PolicyId {PolicyId}",
            context.Message.ClaimId,
            context.Message.PolicyId);

        return Task.CompletedTask;
    }
}
