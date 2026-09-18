using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class CashAccount
{
    public int CashAccountId { get; set; }

    public string AccountName { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public decimal OpeningBalance { get; set; }

    public DateTime OpenedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CashMovement> CashMovements { get; set; } = new List<CashMovement>();

    public virtual ICollection<CourierRemittance> CourierRemittances { get; set; } = new List<CourierRemittance>();
}
