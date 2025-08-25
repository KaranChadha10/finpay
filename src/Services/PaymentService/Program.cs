using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Messaging;
using BuildingBlocks.Observability;
using Contracts.V1.Payments; // from your Shared/Contracts
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Contracts;
using PaymentService.Domain;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Outbox;

var builder = WebApplication.CreateBuilder(args);

// platform
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// observability + messaging
builder.Services.AddFinPayObservability(builder.Configuration);
builder.Services.AddFinPayMessaging(builder.Configuration);

// db
var conn = builder.Configuration.GetConnectionString("Default")
           ?? "Server=localhost;Port=3306;Database=FinPay_Payment;User Id=finpay;Password=finpaypwd;";
builder.Services.AddDbContext<PaymentDbContext>(opt =>
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

builder.Services.AddHostedService<OutboxDispatcher>();
builder.Services.AddHealthChecks();
builder.Services.AddCors(o => o.AddPolicy("AllowAllDev", p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseCors("AllowAllDev");

app.MapGet("/", () => Results.Ok("PaymentService is up"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// --- DEV DB init (fine for local) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// --- Endpoints ---
// NOTE: because the gateway removes '/payments', calling POST /payments forwards to '/' here.
// POST /
app.MapPost("/", async (CreatePaymentDto dto, PaymentDbContext db, IPublishEndpoint bus) =>
{
    var payment = new Payment
    {
        MerchantId = dto.MerchantId,
        Amount = dto.Amount,
        Currency = dto.Currency,
        Method = dto.Method,
        Status = PaymentStatus.Initiated
    };

    db.Payments.Add(payment);
    await db.SaveChangesAsync();

    // build the event
    var evt = new PaymentInitiatedV1(
        payment.PaymentId, payment.MerchantId, payment.Amount, payment.Currency, payment.Method, DateTime.UtcNow);

    // save to Outbox (assembly-qualified type name + JSON payload)
    db.OutboxMessages.Add(new OutboxMessage
    {
        Type = typeof(PaymentInitiatedV1).AssemblyQualifiedName!,
        Payload = System.Text.Json.JsonSerializer.Serialize(evt),
        OccurredOnUtc = DateTime.UtcNow
    });

    await db.SaveChangesAsync();

    return Results.Created($"/{payment.PaymentId}", new { payment.PaymentId, payment.Status });
})
.WithName("CreatePayment")
.Produces(StatusCodes.Status201Created);

// GET /{id}
app.MapGet("/{id:guid}", async (Guid id, PaymentDbContext db) =>
{
    var payment = await db.Payments.FindAsync(id);
    return payment is null ? Results.NotFound() : Results.Ok(payment);
});

app.Run();
