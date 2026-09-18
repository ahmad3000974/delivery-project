using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class CourierCommissionPayment
{
    public long CourierCommissionPaymentId { get; set; }

    public long CourierCommissionId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public string PaymentReference { get; set; } = null!;

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime PaidAt { get; set; }

    public string PaidBy { get; set; } = null!;

    public string? ProofReference { get; set; }

    public virtual CashMovement? CashMovement { get; set; }

    public virtual CourierCommission CourierCommission { get; set; } = null!;
}
