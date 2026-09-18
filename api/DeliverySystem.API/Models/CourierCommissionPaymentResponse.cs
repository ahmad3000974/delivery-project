namespace DeliverySystem.API.Models;

public sealed record CourierCommissionPaymentResponse(
    long CourierCommissionPaymentId,
    long CourierCommissionId,
    Guid IdempotencyKey,
    string PaymentReference,
    decimal Amount,
    string PaymentMethod,
    DateTime PaidAt,
    string PaidBy,
    string? ProofReference);
