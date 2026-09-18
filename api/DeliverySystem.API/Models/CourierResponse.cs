namespace DeliverySystem.API.Models;

public sealed record CourierResponse(
    int CourierId,
    string CourierName,
    string PhoneNumber,
    decimal DefaultCommission,
    bool IsActive,
    DateTime CreatedAt);
