namespace Contracts.V1.Payments;

public record PaymentInitiatedV1(
    Guid PaymentId,
    Guid MerchantId,
    decimal Amount,
    string Currency,
    string Method,
    DateTime CreatedAtUtc
);

public record FraudAssessedV1(
    Guid PaymentId,
    double Score,
    string Recommendation // Approve | Review | Reject
);

public record PaymentAuthorizedV1(Guid PaymentId, DateTime AuthorizedAtUtc);
public record PaymentCaptureRequestedV1(Guid PaymentId);
public record PaymentCapturedV1(Guid PaymentId, DateTime CapturedAtUtc);
public record PaymentFailedV1(Guid PaymentId, string Reason);
