using BuildingBlocks.Messaging;
using BuildingBlocks.Observability;
using FraudService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFinPayObservability(builder.Configuration);

// register our consumer
builder.Services.AddFinPayMessaging(builder.Configuration, x =>
{
    x.AddConsumer<PaymentInitiatedConsumer>();
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok("FraudService is up"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
