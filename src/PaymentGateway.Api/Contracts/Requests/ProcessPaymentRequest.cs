using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Contracts.Requests;

public class ProcessPaymentRequest : IValidatableObject
{
    private static readonly string[] AllowedCurrencies = ["USD", "GBP", "EUR"];

    [Required]
    [RegularExpression(@"\d{14,19}", ErrorMessage = "Card number must be 14-19 digits.")]
    public string? CardNumber { get; set; }

    [Required]
    [Range(1,12)]
    public int? ExpiryMonth { get; set; }

    [Required]
    [Range(1,9999)]
    public int? ExpiryYear { get; set; }

    [Required]
    [StringLength(3, MinimumLength=3, ErrorMessage = "Currency code must be 3 characters long.")]
    public string? CurrencyCode { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive value.")]
    public int? Amount { get; set; }

    [Required]
    [RegularExpression(@"\d{3,4}", ErrorMessage = "CVV must be 3-4 digits.")]
    public string? Cvv { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Data annotations on ExpiryMonth/ExpiryYear individually can't confirm that it's a future date
        // This function runs regardless of whether individual data annotation validations have passed or failed so sense check values
        if (ExpiryMonth is >= 1 and <= 12 && ExpiryYear is >= 1 and <= 9999)
        {
            var expiryEndOfMonth = new DateOnly(ExpiryYear.Value, ExpiryMonth.Value, 1).AddMonths(1).AddDays(-1);
            if (expiryEndOfMonth < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                yield return new ValidationResult(
                    "Card expiry date must be in the future.",
                    new[] { nameof(ExpiryMonth), nameof(ExpiryYear) });
            }
        }

        // 
        if (/*Currency is not null && */!AllowedCurrencies.Contains(CurrencyCode, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                $"Currency must be one of: {string.Join(", ", AllowedCurrencies)}.",
                new[] { nameof(CurrencyCode) });
        }
    }
}