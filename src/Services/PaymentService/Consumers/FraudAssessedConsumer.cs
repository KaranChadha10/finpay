using System;
using Contracts.V1.Payments;
using MassTransit;
using PaymentService.Domain;
using PaymentService.Infrastructure;

namespace PaymentService.Consumers;

public class FraudAssessedConsumer : IConsumer<FraudAssessedV1>
{
    private readonly PaymentDbContext _db;
    public FraudAssessedConsumer(PaymentDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<FraudAssessedV1> ctx)
    {
        var m = ctx.Message;
        var payment = await _db.Payments.FindAsync(m.PaymentId);
        if (payment is null) return;

        payment.FraudScore = m.Score;
        payment.Status = m.Recommendation == "Approve" ? PaymentStatus.Authorized : PaymentStatus.Failed;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}
