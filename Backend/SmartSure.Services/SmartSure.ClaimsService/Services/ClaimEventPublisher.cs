using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.ClaimsService.Services;

public class ClaimEventPublisher(ILogger<ClaimEventPublisher> logger, IPublishEndpoint publishEndpoint) : IClaimEventPublisher
{
    private readonly ILogger<ClaimEventPublisher> _logger = logger;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task PublishClaimSubmittedAsync(ClaimSubmittedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("ClaimSubmitted event published for {ClaimNumber}", eventMessage.ClaimNumber);
    }

    public async Task PublishClaimStatusChangedAsync(ClaimStatusChangedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("ClaimStatusChanged event published for ClaimId {ClaimId}: {OldStatus} -> {NewStatus}", eventMessage.ClaimId, eventMessage.OldStatus, eventMessage.NewStatus);
    }

    public async Task PublishClaimApprovedAsync(ClaimApprovedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("ClaimApproved event published for ClaimId {ClaimId}", eventMessage.ClaimId);
    }

    public async Task PublishClaimRejectedAsync(ClaimRejectedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("ClaimRejected event published for ClaimId {ClaimId}", eventMessage.ClaimId);
    }
}