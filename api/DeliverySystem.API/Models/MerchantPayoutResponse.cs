namespace DeliverySystem.API.Models;

public sealed record MerchantPayoutResponse(
    long MerchantPayoutId,
    long MerchantSettlementId,
    Guid IdempotencyKey,
    string PaymentReference,
    decimal Amount,
    string PaymentMethod,
    DateTime PaidAt,
    string PaidBy,
    string? ProofReference);
