using Contracts.V1.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Domain;

namespace PaymentService.Infrastructure.Capture;

public class CaptureWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CaptureWorker> _logger;

    public CaptureWorker(IServiceScopeFactory scopeFactory, ILogger<CaptureWorker> logger)
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

                // Find a small batch of Authorized payments to capture
                var batch = await db.Payments
                    .Where(p => p.Status == PaymentStatus.Authorized)
                    .OrderBy(p => p.UpdatedAtUtc)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var p in batch)
                {
                    // Simulate external PSP capture
                    await Task.Delay(200, stoppingToken); // pretend network call
                    // succeed always for now

                    p.Status = PaymentStatus.Captured;
                    p.UpdatedAtUtc = DateTime.UtcNow;

                    // Outbox event → will be published by OutboxDispatcher
                    db.OutboxMessages.Add(new Infrastructure.OutboxMessage
                    {
                        Type = typeof(PaymentCapturedV1).AssemblyQualifiedName!,
                        Payload = System.Text.Json.JsonSerializer.Serialize(
                            new PaymentCapturedV1(p.PaymentId, DateTime.UtcNow)),
                        OccurredOnUtc = DateTime.UtcNow
                    });
                }

                if (batch.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CaptureWorker tick failed");
            }
        }
    }
}
