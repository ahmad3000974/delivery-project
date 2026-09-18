using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class MerchantPhone
{
    public int MerchantPhoneId { get; set; }

    public int MerchantId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;
}
