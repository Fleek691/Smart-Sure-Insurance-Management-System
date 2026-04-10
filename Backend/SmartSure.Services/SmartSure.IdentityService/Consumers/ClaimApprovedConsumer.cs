using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Consumers;

public class ClaimApprovedConsumer(ILogger<ClaimApprovedConsumer> logger) : IConsumer<ClaimApprovedEvent>
{
    private readonly ILogger<ClaimApprovedConsumer> _logger = logger;

    public Task Consume(ConsumeContext<ClaimApprovedEvent> context)
    {
        _logger.LogInformation(
            "ClaimApproved consumed for ClaimId {ClaimId}, UserId {UserId}, ApprovedAmount {ApprovedAmount}",
            context.Message.ClaimId,
            context.Message.UserId,
            context.Message.ApprovedAmount);

        return Task.CompletedTask;
    }
}
