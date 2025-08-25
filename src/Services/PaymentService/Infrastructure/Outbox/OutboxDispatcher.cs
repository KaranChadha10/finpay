using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentService.Infrastructure.Outbox;

public class OutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                var bus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                // Fetch a small batch of unprocessed messages
                var batch = await db.OutboxMessages
                    .Where(x => x.ProcessedOnUtc == null)
                    .OrderBy(x => x.OccurredOnUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var msg in batch)
                {
                    try
                    {
                        var type = Type.GetType(msg.Type, throwOnError: true)!;
                        var @event = JsonSerializer.Deserialize(msg.Payload, type)!;
                        await bus.Publish(@event, stoppingToken);

                        msg.ProcessedOnUtc = DateTime.UtcNow;
                        msg.Error = null;
                    }
                    catch (Exception ex)
                    {
                        msg.Attempts += 1;
                        msg.Error = ex.Message;
                        _logger.LogWarning(ex, "Outbox publish failed for {MessageId}", msg.Id);
                    }
                }

                if (batch.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) { /* shutting down */ }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatcher tick failed");
            }
        }
    }
}
