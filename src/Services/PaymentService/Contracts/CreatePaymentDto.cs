namespace PaymentService.Contracts;

using System.ComponentModel.DataAnnotations;

public record CreatePaymentDto(
    [Required] Guid MerchantId,
    [Range(0.01, double.MaxValue)] decimal Amount,
    [Required] string Currency,
    [Required] string Method);