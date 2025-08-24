using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging;
public static class MessagingExtensions
{
    public static IServiceCollection AddFinPayMessaging(this IServiceCollection services, IConfiguration config)
    {
        // TODO: Add MassTransit(RabbitMQ) wiring here
        return services;
    }
}
