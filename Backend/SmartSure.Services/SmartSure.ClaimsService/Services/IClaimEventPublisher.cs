using SmartSure.Shared.Events;

namespace SmartSure.ClaimsService.Services;

public interface IClaimEventPublisher
{
    Task PublishClaimSubmittedAsync(ClaimSubmittedEvent eventMessage);
    Task PublishClaimStatusChangedAsync(ClaimStatusChangedEvent eventMessage);
    Task PublishClaimApprovedAsync(ClaimApprovedEvent eventMessage);
    Task PublishClaimRejectedAsync(ClaimRejectedEvent eventMessage);
}