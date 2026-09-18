using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateAdjustmentRequest
{
    [Required]
    [NotWhiteSpace]
    [StringLength(30)]
    public string TargetType { get; set; } = null!;

    [Range(1, long.MaxValue)]
    public long OriginalId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public decimal AmountDelta { get; set; }

    [Required]
    [NotWhiteSpace]
    [StringLength(500)]
    public string Reason { get; set; } = null!;

    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string CreatedBy { get; set; } = null!;
}
