using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class OrderAssignment
{
    public long OrderAssignmentId { get; set; }

    public long OrderId { get; set; }

    public int CourierId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public string AssignedBy { get; set; } = null!;

    public string? ChangeReason { get; set; }

    public string? HandoverProofReference { get; set; }

    public virtual Courier Courier { get; set; } = null!;

    public virtual ICollection<DeliveryAttempt> DeliveryAttempts { get; set; } = new List<DeliveryAttempt>();

    public virtual DeliveryOrder Order { get; set; } = null!;
}
