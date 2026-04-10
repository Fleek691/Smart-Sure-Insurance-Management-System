using Microsoft.Extensions.Logging;
using MassTransit;
using SmartSure.Shared.Events;

namespace SmartSure.IdentityService.Services;

public class UserRegisteredEventPublisher : IUserRegisteredEventPublisher
{
    private readonly ILogger<UserRegisteredEventPublisher> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public UserRegisteredEventPublisher(ILogger<UserRegisteredEventPublisher> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync(UserRegisteredEvent eventMessage)
    {
        await _publishEndpoint.Publish(eventMessage);
        _logger.LogInformation("UserRegistered event published for {Email}", eventMessage.Email);
    }
}
