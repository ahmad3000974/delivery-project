using System.ComponentModel.DataAnnotations;
using DeliverySystem.API.Validation;

namespace DeliverySystem.API.Models;

public sealed class CreateCollectionRequest
{
    [Range(1, long.MaxValue)]
    public long OrderId { get; set; }
    
    [Range(1, int.MaxValue)]
    public int CourierId{ get; set; }
    
[Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Amount { get; set; }
    
    
    public Guid IdempotencyKey { get; set; }
   [Required]
[NotWhiteSpace]
[StringLength(100)]
public string RecordedBy { get; set; } = null!;
}
