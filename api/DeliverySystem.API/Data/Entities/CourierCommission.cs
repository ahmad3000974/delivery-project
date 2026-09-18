using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class CourierCommission
{
    public long CourierCommissionId { get; set; }

    public long OrderId { get; set; }

    public int CourierId { get; set; }

    public decimal Amount { get; set; }

    public DateTime AccruedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Adjustment> Adjustments { get; set; } = new List<Adjustment>();

    public virtual ICollection<CourierCommissionPayment> CourierCommissionPayments { get; set; } = new List<CourierCommissionPayment>();

    public virtual Courier Courier { get; set; } = null!;

    public virtual DeliveryOrder Order { get; set; } = null!;
}
