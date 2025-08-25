using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddFinPayMessaging(
        this IServiceCollection services,
        IConfiguration config,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        var host = config["RabbitMq:Host"] ?? "localhost";
        var user = config["RabbitMq:Username"] ?? "guest";
        var pass = config["RabbitMq:Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            configure?.Invoke(x);                   // <-- register consumers per-service

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(host, "/", h => { h.Username(user); h.Password(pass); });
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
