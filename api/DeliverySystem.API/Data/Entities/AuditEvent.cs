using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class AuditEvent
{
    public long AuditEventId { get; set; }

    public string EntityType { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? BeforeData { get; set; }

    public string? AfterData { get; set; }

    public DateTime OccurredAt { get; set; }

    public string PerformedBy { get; set; } = null!;

    public Guid? CorrelationId { get; set; }
}
