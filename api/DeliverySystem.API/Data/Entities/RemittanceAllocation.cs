using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class RemittanceAllocation
{
    public long RemittanceAllocationId { get; set; }

    public long CourierRemittanceId { get; set; }

    public long CollectionId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public DateTime AllocatedAt { get; set; }

    public virtual Collection Collection { get; set; } = null!;

    public virtual CourierRemittance CourierRemittance { get; set; } = null!;
}
