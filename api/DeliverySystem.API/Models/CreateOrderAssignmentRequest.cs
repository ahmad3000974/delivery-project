using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateOrderAssignmentRequest
{
    [Range(1, long.MaxValue)]
    public long OrderId { get; set; }

    [Range(1, int.MaxValue)]
    public int CourierId { get; set; }

    [Required]
    [NotWhiteSpace(ErrorMessage = "Assigned by is required.")]
    [StringLength(100)]
    public string AssignedBy { get; set; } = null!;

    [StringLength(500)]
    public string? ChangeReason { get; set; }

    [StringLength(500)]
    public string? HandoverProofReference { get; set; }
}
