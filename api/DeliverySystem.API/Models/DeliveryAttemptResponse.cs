namespace DeliverySystem.API.Models;

public sealed record DeliveryAttemptResponse(
    long DeliveryAttemptId,
    long OrderAssignmentId,
    string Result,
    string? FailureReason,
    DateTime AttemptedAt,
    string? Notes);
