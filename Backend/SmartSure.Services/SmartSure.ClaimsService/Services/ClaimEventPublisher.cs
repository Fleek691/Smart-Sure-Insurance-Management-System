using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.ClaimsService.Services;

/// <summary>
/// Publishes claim domain events to the RabbitMQ message bus via MassTransit.
/// All other services (Admin, Identity) subscribe to these events for audit and notifications.
/// </summary>
public class ClaimEventPublisher(ILogger<ClaimEventPublisher> logger, IPublishEndpoint publishEndpoint) : IClaimEventPublisher
{
    private readonly ILogger<ClaimEventPublisher> _logger = logger;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    /// <summary>Published when a customer submits a Draft claim for review.</summary>
    public async Task PublishClaimSubmittedAsync(ClaimSubmittedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("ClaimSubmitted event published for {ClaimNumber}", eventMessage.ClaimNumber);
    }

    /// <summary>Published on every admin status transition (Submitted→UnderReview, UnderReview→Approved/Rejected).</summary>
    public async Task PublishClaimStatusChangedAsync(ClaimStatusChangedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("ClaimStatusChanged event published for ClaimId {ClaimId}: {OldStatus} -> {NewStatus}", eventMessage.ClaimId, eventMessage.OldStatus, eventMessage.NewStatus);
    }

    /// <summary>Published when an admin approves a claim.</summary>
    public async Task PublishClaimApprovedAsync(ClaimApprovedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("ClaimApproved event published for ClaimId {ClaimId}", eventMessage.ClaimId);
    }

    /// <summary>Published when an admin rejects a claim.</summary>
    public async Task PublishClaimRejectedAsync(ClaimRejectedEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("ClaimRejected event published for ClaimId {ClaimId}", eventMessage.ClaimId);
    }
}