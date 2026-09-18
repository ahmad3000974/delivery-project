using System.ComponentModel.DataAnnotations;

namespace DeliverySystem.API.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotWhiteSpaceAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is string text && !string.IsNullOrWhiteSpace(text);
    }
}
