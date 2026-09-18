namespace DeliverySystem.API.Models;

public sealed record ExpenseResponse(
    long ExpenseId,
    Guid IdempotencyKey,
    string Category,
    decimal Amount,
    string Reason,
    DateTime PaidAt,
    string RecordedBy,
    string ApprovedBy,
    string? ProofReference);
