using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class CashMovement
{
    public long CashMovementId { get; set; }

    public int CashAccountId { get; set; }

    public string SourceType { get; set; } = null!;

    public long? CourierRemittanceId { get; set; }

    public long? MerchantPayoutId { get; set; }

    public long? CommissionPaymentId { get; set; }

    public long? ExpenseId { get; set; }

    public long? AdjustmentId { get; set; }

    public string Direction { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime OccurredAt { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual Adjustment? Adjustment { get; set; }

    public virtual ICollection<Adjustment> Adjustments { get; set; } = new List<Adjustment>();

    public virtual CashAccount CashAccount { get; set; } = null!;

    public virtual CourierCommissionPayment? CommissionPayment { get; set; }

    public virtual CourierRemittance? CourierRemittance { get; set; }

    public virtual Expense? Expense { get; set; }

    public virtual MerchantPayout? MerchantPayout { get; set; }
}
