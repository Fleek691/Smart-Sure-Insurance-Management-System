using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Consumers;

/// <summary>
/// Listens for <see cref="ClaimRejectedEvent"/> messages from the ClaimsService.
/// Currently logs the rejection. Extend to notify users with the rejection reason.
/// </summary>
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
