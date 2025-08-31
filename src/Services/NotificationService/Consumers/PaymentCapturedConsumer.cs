using Contracts.V1.Payments;
using MailKit.Net.Smtp;
using MassTransit;
using MimeKit;

namespace NotificationService.Consumers;

public class PaymentCapturedConsumer : IConsumer<PaymentCapturedV1>
{
    private readonly IConfiguration _config;
    public PaymentCapturedConsumer(IConfiguration config) => _config = config;

    public async Task Consume(ConsumeContext<PaymentCapturedV1> ctx)
    {
        var msg = ctx.Message;

        var to = _config["Notifications:ToEmail"] ?? "merchant@example.com";
        var from = _config["Notifications:FromEmail"] ?? "no-reply@finpay.local";
        var host = _config["Notifications:SmtpHost"] ?? "localhost";
        var port = int.TryParse(_config["Notifications:SmtpPort"], out var p) ? p : 1025;

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(from));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = $"Payment Captured: {msg.PaymentId}";
        email.Body = new TextPart("plain")
        {
            Text = $"Your payment {msg.PaymentId} was captured at {msg.CapturedAtUtc:u}."
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, useSsl: false, ctx.CancellationToken);
        // MailHog needs no auth
        await client.SendAsync(email, ctx.CancellationToken);
        await client.DisconnectAsync(true, ctx.CancellationToken);
    }


}
