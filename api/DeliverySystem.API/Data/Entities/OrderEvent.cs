using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class OrderEvent
{
    public long OrderEventId { get; set; }

    public long OrderId { get; set; }

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public DateTime OccurredAt { get; set; }

    public DateTime RecordedAt { get; set; }

    public string RecordedBy { get; set; } = null!;

    public string? Reason { get; set; }

    public virtual DeliveryOrder Order { get; set; } = null!;
}
