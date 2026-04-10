using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Consumers;

public class ClaimStatusChangedConsumer(ILogger<ClaimStatusChangedConsumer> logger) : IConsumer<ClaimStatusChangedEvent>
{
    private readonly ILogger<ClaimStatusChangedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<ClaimStatusChangedEvent> context)
    {
        _logger.LogInformation(
            "ClaimStatusChanged consumed for ClaimId {ClaimId}: {OldStatus} -> {NewStatus}",
            context.Message.ClaimId,
            context.Message.OldStatus,
            context.Message.NewStatus);

        return Task.CompletedTask;
    }
}
