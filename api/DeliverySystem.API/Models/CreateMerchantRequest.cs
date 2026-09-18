using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateMerchantRequest
{
    [Required]
    [NotWhiteSpace(ErrorMessage = "Merchant name is required.")]
    [StringLength(150)]
    public string MerchantName { get; init; } = null!;

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal DefaultMerchantFee { get; init; }

    public bool IsActive { get; init; } = true;
}
