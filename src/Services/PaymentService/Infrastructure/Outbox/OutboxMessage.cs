using System;

namespace PaymentService.Infrastructure;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;          // assembly-qualified .NET type name
    public string Payload { get; set; } = default!;       // JSON
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
}
