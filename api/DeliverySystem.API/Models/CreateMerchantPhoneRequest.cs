using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateMerchantPhoneRequest
{
    [Range(1, int.MaxValue)]
    public int MerchantId { get; set; }

    [Required]
    [NotWhiteSpace]
    [StringLength(25)]
    public string PhoneNumber { get; set; } = null!;

    public bool IsPrimary { get; set; }
}
