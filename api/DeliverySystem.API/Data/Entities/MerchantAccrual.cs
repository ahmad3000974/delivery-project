using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class MerchantAccrual
{
    public long MerchantAccrualId { get; set; }

    public long OrderId { get; set; }

    public decimal MerchantAmount { get; set; }

    public decimal CompanyFee { get; set; }

    public DateTime AccruedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Adjustment> Adjustments { get; set; } = new List<Adjustment>();

    public virtual MerchantSettlementLine? MerchantSettlementLine { get; set; }

    public virtual DeliveryOrder Order { get; set; } = null!;
}
