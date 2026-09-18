using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class UpdateCompanySettingRequest
{
    [Required]
    [NotWhiteSpace]
    [StringLength(150)]
    public string CompanyName { get; set; } = null!;

    [Required]
    [NotWhiteSpace]
    [StringLength(3, MinimumLength = 3)]
    public string CurrencyCode { get; set; } = null!;

    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string TimeZoneId { get; set; } = null!;

    [Required]
    [NotWhiteSpace]
    [StringLength(20)]
    public string OrderNumberPrefix { get; set; } = null!;
}
