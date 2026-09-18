using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class ReverseAdjustmentRequest
{
    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string CreatedBy { get; set; } = null!;
}
