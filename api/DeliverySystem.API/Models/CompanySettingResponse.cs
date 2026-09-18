namespace DeliverySystem.API.Models;

public sealed record CompanySettingResponse(
    int CompanySettingsId,
    string CompanyName,
    string CurrencyCode,
    string TimeZoneId,
    string OrderNumberPrefix,
    long NextOrderNumber);
