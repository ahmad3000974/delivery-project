using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class Adjustment
{
    public long AdjustmentId { get; set; }

    public string TargetType { get; set; } = null!;

    public long? OriginalCollectionId { get; set; }

    public long? OriginalAccrualId { get; set; }

    public long? OriginalCommissionId { get; set; }

    public long? OriginalCashMovementId { get; set; }

    public long? ReversesAdjustmentId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public decimal AmountDelta { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? ApprovedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual CashMovement? CashMovement { get; set; }

    public virtual ICollection<Adjustment> InverseReversesAdjustment { get; set; } = new List<Adjustment>();

    public virtual MerchantAccrual? OriginalAccrual { get; set; }

    public virtual CashMovement? OriginalCashMovement { get; set; }

    public virtual Collection? OriginalCollection { get; set; }

    public virtual CourierCommission? OriginalCommission { get; set; }

    public virtual Adjustment? ReversesAdjustment { get; set; }
}
