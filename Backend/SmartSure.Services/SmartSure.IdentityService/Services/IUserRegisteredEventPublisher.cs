using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Services;

public interface IUserRegisteredEventPublisher
{
    Task PublishAsync(UserRegisteredEvent eventMessage);
}
