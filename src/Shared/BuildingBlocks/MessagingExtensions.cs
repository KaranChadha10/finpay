using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging;
public static class MessagingExtensions
{
    public static IServiceCollection AddFinPayMessaging(this IServiceCollection services, IConfiguration config)
    {
        var host = config["RabbitMq:Host"] ?? "localhost";
        var user = config["RabbitMq:Username"] ?? "guest";
        var pass = config["RabbitMq:Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            // Consumers will be added per service later (x.AddConsumer<...>())
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(host, "/", h =>
                {
                    h.Username(user);
                    h.Password(pass);
                });

                // optional: service instance name for observability
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
