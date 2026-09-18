using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class MerchantSettlementLine
{
    public long MerchantSettlementLineId { get; set; }

    public long MerchantSettlementId { get; set; }

    public long MerchantAccrualId { get; set; }

    public decimal SettlementAmount { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public virtual MerchantAccrual MerchantAccrual { get; set; } = null!;

    public virtual MerchantSettlement MerchantSettlement { get; set; } = null!;
}
