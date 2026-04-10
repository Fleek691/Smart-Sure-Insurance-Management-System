using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Consumers;

public class ClaimRejectedConsumer(ILogger<ClaimRejectedConsumer> logger) : IConsumer<ClaimRejectedEvent>
{
    private readonly ILogger<ClaimRejectedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<ClaimRejectedEvent> context)
    {
        _logger.LogInformation(
            "ClaimRejected consumed for ClaimId {ClaimId}, UserId {UserId}, Reason {Reason}",
            context.Message.ClaimId,
            context.Message.UserId,
            context.Message.Reason);

        return Task.CompletedTask;
    }
}
