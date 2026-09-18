namespace DeliverySystem.API.Models;

public sealed record MerchantResponse(
    int MerchantId,
    string MerchantName,
    decimal DefaultMerchantFee,
    bool IsActive,
    DateTime CreatedAt);
