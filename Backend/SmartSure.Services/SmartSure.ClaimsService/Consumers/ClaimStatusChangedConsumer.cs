using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.ClaimsService.Consumers;

/// <summary>
/// Listens for <see cref="ClaimStatusChangedEvent"/> messages published by the ClaimsService itself.
/// Currently logs the transition for observability.
/// Extend to trigger side-effects such as sending customer notifications.
/// </summary>
public class ClaimStatusChangedConsumer(ILogger<ClaimStatusChangedConsumer> logger) : IConsumer<ClaimStatusChangedEvent>
{
    private readonly ILogger<ClaimStatusChangedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<ClaimStatusChangedEvent> context)
    {
        _logger.LogInformation(
            "ClaimStatusChanged consumed in ClaimsService for ClaimId {ClaimId}: {OldStatus} -> {NewStatus}",
            context.Message.ClaimId,
            context.Message.OldStatus,
            context.Message.NewStatus);

        return Task.CompletedTask;
    }
}
