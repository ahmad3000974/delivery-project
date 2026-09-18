using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class RemittanceAllocationItem
{
    [Range(1, long.MaxValue)]
    public long CollectionId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal AllocatedAmount { get; set; }
}

public sealed class CreateCourierRemittanceRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int CourierId { get; set; }

    [Range(1, int.MaxValue)]
    public int CashAccountId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Amount { get; set; }

    public Guid IdempotencyKey { get; set; }

    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string ReceivedBy { get; set; } = null!;

    [StringLength(500)]
    public string? ProofReference { get; set; }

    [Required]
    [MinLength(1)]
    public List<RemittanceAllocationItem> Allocations { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Allocations is null || Allocations.Count == 0)
        {
            yield break;
        }

        var ids = Allocations.Select(item => item.CollectionId).ToList();
        if (ids.Count != ids.Distinct().Count())
        {
            yield return new ValidationResult(
                "Each collection can appear only once in a remittance.",
                [nameof(Allocations)]);
        }

        var sum = Allocations.Sum(item => item.AllocatedAmount);
        if (sum != Amount)
        {
            yield return new ValidationResult(
                "Allocated amounts must equal the remittance amount.",
                [nameof(Allocations), nameof(Amount)]);
        }
    }
}
