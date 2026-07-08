using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Extensions;

namespace PaymentGateway.Api.Tests.Unit.Extensions;

public class PaymentMappingExtensionsTests
{
    [Fact]
    public void ToDomain_MapsAllFieldsFromRequest()
    {
        var request = new ProcessPaymentRequest
        {
            CardNumber = "1234123412341234",
            ExpiryMonth = 8,
            ExpiryYear = 2027,
            CurrencyCode = "GBP",
            Amount = 999,
            Cvv = "123",
        };

        var payment = request.ToDomain();

        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(PaymentStatus.None, payment.Status);
        Assert.Equal(request.CardNumber, payment.CardNumber);
        Assert.Equal(request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(request.CurrencyCode, payment.CurrencyCode);
        Assert.Equal(request.Amount, payment.Amount);
        Assert.Equal(request.Cvv, payment.Cvv);
    }

    [Theory]
    [InlineData("1234123412341234", "1234")]
    [InlineData("12", "12")]
    [InlineData("", "")]
    public void ToResponse_ReturnsCorrectCardNumberLastFour(string cardNumber, string? expected)
    {
        var payment = new Payment { CardNumber = cardNumber, CurrencyCode = "GBP", Cvv = "123" };

        var response = payment.ToResponse();

        Assert.Equal(expected, response.CardNumberLastFour);
    }

    [Fact]
    public void ToRejectedResponse_BuildsResponseWithoutId_WhenRequestIsNull()
    {
        var response = ((ProcessPaymentRequest?)null).ToRejectedResponse(new[] { "The request body is invalid or could not be parsed." });

        Assert.Null(response.Id);
        Assert.Equal(PaymentStatus.Rejected, response.Status);
        Assert.Equal(new[] { "The request body is invalid or could not be parsed." }, response.RejectionReasons);
        Assert.Equal("", response.CardNumberLastFour);
        Assert.Equal(0, response.ExpiryMonth);
        Assert.Equal(0, response.ExpiryYear);
        Assert.Null(response.CurrencyCode);
        Assert.Equal(0, response.Amount);
    }

    [Fact]
    public void ToRejectedResponse_PreservesAvailableFields_WhenRequestIsPartiallyValid()
    {
        var request = new ProcessPaymentRequest
        {
            CardNumber = "1234123412341234",
            ExpiryMonth = 1,
            ExpiryYear = 2020, // expired
            CurrencyCode = "XXX", // not allowed
            Amount = 500,
            Cvv = "123",
        };

        var response = request.ToRejectedResponse(new[] { "Card expiry date must be in the future." });

        Assert.Equal("1234", response.CardNumberLastFour);
        Assert.Equal(500, response.Amount);
        Assert.Equal("XXX", response.CurrencyCode);
    }

    [Fact]
    public void ToBankClientRequest_MapsFieldsWithZeroPaddedExpiryMonth()
    {
        var payment = new Payment
        {
            CardNumber = "1234123412341234",
            ExpiryMonth = 3,
            ExpiryYear = 2026,
            CurrencyCode = "GBP",
            Amount = 500,
            Cvv = "123",
        };

        var bankRequest = payment.ToBankClientRequest();

        Assert.Equal("1234123412341234", bankRequest.CardNumber);
        Assert.Equal("03/2026", bankRequest.ExpiryDate);
        Assert.Equal("GBP", bankRequest.Currency);
        Assert.Equal(500, bankRequest.Amount);
        Assert.Equal("123", bankRequest.Cvv);
    }
}
