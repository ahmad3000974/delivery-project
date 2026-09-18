using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class Courier
{
    public int CourierId { get; set; }

    public string CourierName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public decimal DefaultCommission { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

    public virtual ICollection<CourierCommission> CourierCommissions { get; set; } = new List<CourierCommission>();

    public virtual ICollection<CourierRemittance> CourierRemittances { get; set; } = new List<CourierRemittance>();

    public virtual ICollection<OrderAssignment> OrderAssignments { get; set; } = new List<OrderAssignment>();
}
