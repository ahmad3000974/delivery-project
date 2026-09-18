using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateMerchantSettlementRequest
{
    [Range(1, int.MaxValue)]
    public int MerchantId { get; set; }

    [Required]
    [NotWhiteSpace]
    [StringLength(100)]
    public string CreatedBy { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public List<long> MerchantAccrualIds { get; set; } = null!;
}
