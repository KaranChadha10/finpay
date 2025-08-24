using BuildingBlocks.Messaging;
using BuildingBlocks.Observability;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Load base settings + YARP routes
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.yarp.json", optional: false, reloadOnChange: true);

// Platform services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Observability & messaging (safe no-ops until wired)
builder.Services.AddFinPayObservability(builder.Configuration);
builder.Services.AddFinPayMessaging(builder.Configuration);

// YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Dev CORS (handy if a UI calls the gateway)
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowAllDev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // keep off for HTTP-only local runs
app.UseCors("AllowAllDev");

// Friendly root + health
app.MapGet("/", () => Results.Ok("ApiGateway is up"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Reverse proxy
app.MapReverseProxy();

app.Run();
