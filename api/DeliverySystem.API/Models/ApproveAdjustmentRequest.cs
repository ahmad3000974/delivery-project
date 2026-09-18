using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class ApproveAdjustmentRequest
{
    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string ApprovedBy { get; set; } = null!;
}
