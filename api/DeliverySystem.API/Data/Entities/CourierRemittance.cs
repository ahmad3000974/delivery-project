using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class CourierRemittance
{
    public long CourierRemittanceId { get; set; }

    public int CourierId { get; set; }

    public int CashAccountId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public decimal Amount { get; set; }

    public string ReceivedBy { get; set; } = null!;

    public DateTime RemittedAt { get; set; }

    public string? ProofReference { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual CashAccount CashAccount { get; set; } = null!;

    public virtual CashMovement? CashMovement { get; set; }

    public virtual Courier Courier { get; set; } = null!;

    public virtual ICollection<RemittanceAllocation> RemittanceAllocations { get; set; } = new List<RemittanceAllocation>();
}
