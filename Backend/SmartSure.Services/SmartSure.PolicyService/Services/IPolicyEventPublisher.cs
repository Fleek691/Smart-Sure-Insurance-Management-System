using SmartSure.Shared.Events;

namespace SmartSure.PolicyService.Services;

public interface IPolicyEventPublisher
{
    Task PublishActivatedAsync(PolicyActivatedEvent eventMessage);
    Task PublishCancelledAsync(PolicyCancelledEvent eventMessage);
}