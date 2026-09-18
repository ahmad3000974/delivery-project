using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class Collection
{
    public long CollectionId { get; set; }

    public long OrderId { get; set; }

    public int CourierId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public decimal Amount { get; set; }

    public DateTime CollectedAt { get; set; }

    public string RecordedBy { get; set; } = null!;

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Adjustment> Adjustments { get; set; } = new List<Adjustment>();

    public virtual Courier Courier { get; set; } = null!;

    public virtual DeliveryOrder Order { get; set; } = null!;

    public virtual ICollection<RemittanceAllocation> RemittanceAllocations { get; set; } = new List<RemittanceAllocation>();
}
