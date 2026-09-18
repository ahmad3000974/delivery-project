namespace DeliverySystem.API.Models;

public sealed record MerchantSettlementResponse(
    long MerchantSettlementId,
    int MerchantId,
    string Status,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ApprovedAt,
    string? ApprovedBy);
