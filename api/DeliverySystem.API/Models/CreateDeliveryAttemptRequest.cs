using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateDeliveryAttemptRequest : IValidatableObject
{
    [Range(1, long.MaxValue)]
    public long OrderAssignmentId { get; set; }

    [Required]
    [NotWhiteSpace(ErrorMessage = "Result is required.")]
    [StringLength(30)]
    public string Result { get; set; } = null!;

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var result = Result?.Trim();

        if (result is not ("Delivered" or "Failed" or "Rescheduled"))
        {
            yield return new ValidationResult(
                "Result must be Delivered, Failed, or Rescheduled.",
                [nameof(Result)]);
        }

        if (result == "Failed" && string.IsNullOrWhiteSpace(FailureReason))
        {
            yield return new ValidationResult(
                "Failure reason is required when the result is Failed.",
                [nameof(FailureReason)]);
        }
    }
}
