using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class DeliveryOrder
{
    public long OrderId { get; set; }

    public int MerchantId { get; set; }

    public string InternalOrderNumber { get; set; } = null!;

    public string? ExternalReference { get; set; }

    public string RecipientName { get; set; } = null!;

    public string RecipientPhoneNumber { get; set; } = null!;

    public string DeliveryAddress { get; set; } = null!;

    public decimal GoodsAmount { get; set; }

    public decimal RecipientFee { get; set; }

    public decimal MerchantFee { get; set; }

    public decimal CourierCommissionAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Collection? Collection { get; set; }

    public virtual CourierCommission? CourierCommission { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual MerchantAccrual? MerchantAccrual { get; set; }

    public virtual ICollection<OrderAssignment> OrderAssignments { get; set; } = new List<OrderAssignment>();

    public virtual ICollection<OrderEvent> OrderEvents { get; set; } = new List<OrderEvent>();
}
