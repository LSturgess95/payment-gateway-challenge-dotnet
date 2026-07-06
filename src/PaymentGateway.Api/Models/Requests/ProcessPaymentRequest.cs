using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

public class ProcessPaymentRequest
{
    [Required]
    [RegularExpression(@"\d{14,19}")]
    public string CardNumber { get; set; }

    [Required]
    [Range(1,12)]
    public int ExpiryMonth { get; set; }

    [Required]
    // Must be in the future - Ensure the combination of expiry month + year is in the future
    public int ExpiryYear { get; set; }

    [Required]
    [StringLength(3,MinimumLength=3)]
    // Refer to the list of ISO currency codes. Ensure your submission validates against no more than 3 currency codes
    public string Currency { get; set; }

    [Required]
    public int Amount { get; set; }

    [Required]
    [RegularExpression(@"\d{3,4}")]
    public int Cvv { get; set; }
}