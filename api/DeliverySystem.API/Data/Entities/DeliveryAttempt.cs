using System;
using System.Collections.Generic;

namespace DeliverySystem.API.Data.Entities;

public partial class DeliveryAttempt
{
    public long DeliveryAttemptId { get; set; }

    public long OrderAssignmentId { get; set; }

    public string Result { get; set; } = null!;

    public string? FailureReason { get; set; }

    public DateTime AttemptedAt { get; set; }

    public string? Notes { get; set; }

    public virtual OrderAssignment OrderAssignment { get; set; } = null!;
}
