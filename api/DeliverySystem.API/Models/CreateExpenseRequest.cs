using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateExpenseRequest
{
    [Range(1, int.MaxValue)]
    public int CashAccountId { get; set; }

    public Guid IdempotencyKey { get; set; }

    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string Category { get; set; } = null!;

    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Amount { get; set; }

    [Required]
    [NotWhiteSpace]
    [StringLength(500)]
    public string Reason { get; set; } = null!;

    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string RecordedBy { get; set; } = null!;

    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string ApprovedBy { get; set; } = null!;

    [StringLength(500)]
    public string? ProofReference { get; set; }
}
