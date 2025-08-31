using BuildingBlocks.Messaging;
using BuildingBlocks.Observability;
using NotificationService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFinPayObservability(builder.Configuration);

// MassTransit with our consumer
builder.Services.AddFinPayMessaging(builder.Configuration, x =>
{
    x.AddConsumer<PaymentCapturedConsumer>();
});
// basic config for MailHog (override via appsettings if you like)
builder.Configuration["Notifications:SmtpHost"] = builder.Configuration["Notifications:SmtpHost"] ?? "localhost";
builder.Configuration["Notifications:SmtpPort"] = builder.Configuration["Notifications:SmtpPort"] ?? "1025";
builder.Configuration["Notifications:FromEmail"] = builder.Configuration["Notifications:FromEmail"] ?? "no-reply@finpay.local";
builder.Configuration["Notifications:ToEmail"] = builder.Configuration["Notifications:ToEmail"] ?? "merchant@example.com";

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok("NotificationService is up"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();