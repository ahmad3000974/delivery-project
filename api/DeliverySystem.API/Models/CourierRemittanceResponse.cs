namespace DeliverySystem.API.Models;

public sealed record CourierRemittanceResponse(
    long CourierRemittanceId,
    int CourierId,
    int CashAccountId,
    Guid IdempotencyKey,
    decimal Amount,
    string ReceivedBy,
    DateTime RemittedAt,
    string? ProofReference);
