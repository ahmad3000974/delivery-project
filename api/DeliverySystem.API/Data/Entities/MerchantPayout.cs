using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class MerchantPayout
{
    public long MerchantPayoutId { get; set; }

    public long MerchantSettlementId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public string PaymentReference { get; set; } = null!;

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime PaidAt { get; set; }

    public string PaidBy { get; set; } = null!;

    public string? ProofReference { get; set; }

    public virtual CashMovement? CashMovement { get; set; }

    public virtual MerchantSettlement MerchantSettlement { get; set; } = null!;
}
