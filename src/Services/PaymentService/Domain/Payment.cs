using System;

namespace PaymentService.Domain;

public enum PaymentStatus { Initiated = 0, Authorized = 1, Captured = 2, Failed = 3, Refunded = 4 }

public class Payment
{
    public Guid PaymentId { get; set; } = Guid.NewGuid();
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Method { get; set; } = "Card";
    public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;
    public double? FraudScore { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
