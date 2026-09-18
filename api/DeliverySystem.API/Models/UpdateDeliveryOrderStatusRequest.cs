using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class UpdateDeliveryOrderStatusRequest
{
    [Required]
    [NotWhiteSpace(ErrorMessage = "Status is required.")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [StringLength(500)]
    public string? Reason { get; set; }
}
