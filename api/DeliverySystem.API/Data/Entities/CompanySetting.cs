using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class CompanySetting
{
    public int CompanySettingsId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string CurrencyCode { get; set; } = null!;

    public string TimeZoneId { get; set; } = null!;

    public string OrderNumberPrefix { get; set; } = null!;

    public long NextOrderNumber { get; set; }

    public byte[] RowVersion { get; set; } = null!;
}
