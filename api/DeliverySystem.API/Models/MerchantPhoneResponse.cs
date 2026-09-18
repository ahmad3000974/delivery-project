namespace DeliverySystem.API.Models;

public sealed record MerchantPhoneResponse(
    int MerchantPhoneId,
    int MerchantId,
    string PhoneNumber,
    bool IsPrimary);
