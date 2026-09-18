using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateDeliveryOrderRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int MerchantId { get; set; }

    [StringLength(40)]
    public string? InternalOrderNumber { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    [Required]
    [NotWhiteSpace(ErrorMessage = "Recipient name is required.")]
    [StringLength(150)]
    public string RecipientName { get; set; } = null!;

    [Required]
    [NotWhiteSpace(ErrorMessage = "Recipient phone number is required.")]
    [StringLength(25)]
    public string RecipientPhoneNumber { get; set; } = null!;

    [Required]
    [NotWhiteSpace(ErrorMessage = "Delivery address is required.")]
    [StringLength(500)]
    public string DeliveryAddress { get; set; } = null!;

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal GoodsAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal RecipientFee { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal MerchantFee { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal CourierCommissionAmount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (InternalOrderNumber is not null && string.IsNullOrWhiteSpace(InternalOrderNumber))
        {
            yield return new ValidationResult(
                "Internal order number must contain non-whitespace characters when supplied.",
                [nameof(InternalOrderNumber)]);
        }

        if (ExternalReference is not null && string.IsNullOrWhiteSpace(ExternalReference))
        {
            yield return new ValidationResult(
                "External reference must contain non-whitespace characters when supplied.",
                [nameof(ExternalReference)]);
        }

        if (MerchantFee > GoodsAmount)
        {
            yield return new ValidationResult(
                "Merchant fee cannot exceed goods amount.",
                [nameof(MerchantFee)]);
        }

        if (GoodsAmount + RecipientFee <= 0)
        {
            yield return new ValidationResult(
                "Goods amount plus recipient fee must be greater than zero.",
                [nameof(GoodsAmount), nameof(RecipientFee)]);
        }
    }
}