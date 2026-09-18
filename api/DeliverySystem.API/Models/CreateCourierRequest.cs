using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateCourierRequest
{
    [Required]
    [NotWhiteSpace(ErrorMessage = "Courier name is required.")]
    [StringLength(150)]
    public string CourierName { get; init; } = null!;

    [Required]
    [NotWhiteSpace(ErrorMessage = "Phone number is required.")]
    [StringLength(25)]
    public string PhoneNumber { get; init; } = null!;

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal DefaultCommission { get; init; }

    public bool IsActive { get; init; } = true;
}
