namespace DeliverySystem.API.Models;

public sealed record DeliveryOrderResponse(
    long OrderId,
    int MerchantId,
    string InternalOrderNumber,
    string? ExternalReference,
    string RecipientName,
    string RecipientPhoneNumber,
    string DeliveryAddress,
    decimal GoodsAmount,
    decimal RecipientFee,
    decimal MerchantFee,
    decimal CourierCommissionAmount,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
