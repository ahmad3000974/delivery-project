namespace DeliverySystem.API.Models;

public sealed record OrderAssignmentResponse(
    long OrderAssignmentId,
    long OrderId,
    int CourierId,
    DateTime StartedAt,
    DateTime? EndedAt,
    string AssignedBy,
    string? ChangeReason,
    string? HandoverProofReference);
