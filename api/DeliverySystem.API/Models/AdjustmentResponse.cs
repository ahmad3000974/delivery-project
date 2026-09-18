namespace DeliverySystem.API.Models;

public sealed record AdjustmentResponse(
    long AdjustmentId,
    string TargetType,
    long? OriginalCollectionId,
    long? OriginalAccrualId,
    long? OriginalCommissionId,
    long? OriginalCashMovementId,
    long? ReversesAdjustmentId,
    Guid IdempotencyKey,
    decimal AmountDelta,
    string Reason,
    string Status,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ApprovedAt,
    string? ApprovedBy);
