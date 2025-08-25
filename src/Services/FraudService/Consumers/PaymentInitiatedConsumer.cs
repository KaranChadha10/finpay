using Contracts.V1.Payments;
using MassTransit;

namespace FraudService.Consumers;

public class PaymentInitiatedConsumer : IConsumer<PaymentInitiatedV1>
{
    public async Task Consume(ConsumeContext<PaymentInitiatedV1> ctx)
    {
        var m = ctx.Message;
        // simple rule: always approve for now
        await ctx.Publish(new FraudAssessedV1(m.PaymentId, 0.05, "Approve"));
    }
}
