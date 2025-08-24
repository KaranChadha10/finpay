using BuildingBlocks.Messaging;
using BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args);

// --- Platform services ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Observability & messaging (safe no-ops until you wire them)
builder.Services.AddFinPayObservability(builder.Configuration);
builder.Services.AddFinPayMessaging(builder.Configuration);

// Health checks + CORS (dev-friendly)
builder.Services.AddHealthChecks();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAllDev", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

// --- Middleware pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// For local dev, keep HTTPS redirection off unless you bind https as well
// app.UseHttpsRedirection();

app.UseCors("AllowAllDev");

// --- Minimal endpoints ---
app.MapGet("/", () => Results.Ok("MerchantService is up"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Liveness/readiness (same as /health for now; you can evolve later)
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
