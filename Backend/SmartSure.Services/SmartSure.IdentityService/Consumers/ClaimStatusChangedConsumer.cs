using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Consumers;

/// <summary>
/// Listens for <see cref="ClaimStatusChangedEvent"/> messages from the ClaimsService.
/// Currently logs the transition. Extend to notify users of status changes in real time.
/// </summary>
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
