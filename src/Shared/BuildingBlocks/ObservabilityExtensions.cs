using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Observability;
public static class ObservabilityExtensions
{
    public static IServiceCollection AddFinPayObservability(this IServiceCollection services, IConfiguration config)
    {
        // TODO: Add Serilog + OpenTelemetry wiring here
        return services;
    }
}
