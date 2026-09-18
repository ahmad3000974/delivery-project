using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class Expense
{
    public long ExpenseId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public string Category { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime PaidAt { get; set; }

    public string RecordedBy { get; set; } = null!;

    public string ApprovedBy { get; set; } = null!;

    public string? ProofReference { get; set; }

    public virtual CashMovement? CashMovement { get; set; }
}
