using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Contracts.Requests;

namespace PaymentGateway.Api.Tests.Unit.Contracts;

public class ProcessPaymentRequestValidationTests
{
    private static ProcessPaymentRequest ValidRequest() => new()
    {
        CardNumber = "1234123412341234",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        CurrencyCode = "GBP",
        Amount = 500,
        Cvv = "123",
    };

    // Immitates the automatic validation done by data annotations
    private static List<ValidationResult> Validate(ProcessPaymentRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidRequest_ProducesNoValidationErrors()
    {
        var results = Validate(ValidRequest());

        Assert.Empty(results);
    }

    [Fact]
    public void MissingCardNumber_ProducesRequiredError()
    {
        var request = ValidRequest();
        request.CardNumber = null;

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProcessPaymentRequest.CardNumber)));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901234567890")]
    [InlineData("abcd1234123412341234")]
    public void InvalidCardNumberFormat_ProducesRegularExpressionError(string cardNumber)
    {
        var request = ValidRequest();
        request.CardNumber = cardNumber;

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProcessPaymentRequest.CardNumber)));
    }

    [Fact]
    public void ExpiryMonthOutOfRange_ProducesRangeError()
    {
        var request = ValidRequest();
        request.ExpiryMonth = 13;

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProcessPaymentRequest.ExpiryMonth)));
    }

    [Fact]
    public void ExpiredCard_ProducesFutureDateError()
    {
        var request = ValidRequest();
        request.ExpiryMonth = 1;
        request.ExpiryYear = 2020;

        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage == "Card expiry date must be in the future.");
    }

    [Fact]
    public void MissingExpiryFields_DoesNotThrow_AndSkipsFutureDateCheck()
    {
        var request = ValidRequest();
        request.ExpiryMonth = null;
        request.ExpiryYear = null;

        var results = Validate(request);

        Assert.DoesNotContain(results, r => r.ErrorMessage == "Card expiry date must be in the future.");
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProcessPaymentRequest.ExpiryMonth)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProcessPaymentRequest.ExpiryYear)));
    }

    [Fact]
    public void DisallowedCurrency_ProducesCurrencyError()
    {
        var request = ValidRequest();
        request.CurrencyCode = "AUD";

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProcessPaymentRequest.CurrencyCode)));
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("GBP")]
    [InlineData("EUR")]
    [InlineData("gbp")]
    [InlineData("GbP")]
    public void AllowedCurrency_IsCaseInsensitive_AndProducesNoCurrencyError(string currency)
    {
        var request = ValidRequest();
        request.CurrencyCode = currency;

        var results = Validate(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(ProcessPaymentRequest.CurrencyCode)));
    }

    [Fact]
    public void InvalidCvvFormat_ProducesRegularExpressionError()
    {
        var request = ValidRequest();
        request.Cvv = "12";

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProcessPaymentRequest.Cvv)));
    }
}
