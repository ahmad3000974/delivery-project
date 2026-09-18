using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class MerchantSettlement
{
    public long MerchantSettlementId { get; set; }

    public int MerchantId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? ApprovedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual ICollection<MerchantPayout> MerchantPayouts { get; set; } = new List<MerchantPayout>();

    public virtual ICollection<MerchantSettlementLine> MerchantSettlementLines { get; set; } = new List<MerchantSettlementLine>();
}
