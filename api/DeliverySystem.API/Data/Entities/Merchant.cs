using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class Merchant
{
    public int MerchantId { get; set; }

    public string MerchantName { get; set; } = null!;

    public decimal DefaultMerchantFee { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<DeliveryOrder> DeliveryOrders { get; set; } = new List<DeliveryOrder>();

    public virtual ICollection<MerchantPhone> MerchantPhones { get; set; } = new List<MerchantPhone>();

    public virtual ICollection<MerchantSettlement> MerchantSettlements { get; set; } = new List<MerchantSettlement>();
}
