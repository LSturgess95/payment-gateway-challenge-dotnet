using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;
using Moq.Protected;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Clients.Exceptions;
using PaymentGateway.Api.Contracts.Requests;

namespace PaymentGateway.Api.Tests.Unit.Clients;

public class BankClientTests
{
    private static BankPaymentRequest ValidRequest() => new()
    {
        CardNumber = "1234123412341234",
        ExpiryDate = "12/2026",
        Currency = "GBP",
        Amount = 500,
        Cvv = "123",
    };

    private static BankClient CreateSut(HttpStatusCode statusCode, object? responseBody)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = responseBody is null ? null : JsonContent.Create(responseBody),
            });

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost:8080") };
        return new BankClient(httpClient, NullLogger<BankClient>.Instance);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsBankResponse_WhenBankReturnsSuccess()
    {
        var sut = CreateSut(HttpStatusCode.OK, new { authorized = true, authorization_code = "abc-123" });

        var result = await sut.ProcessPaymentAsync(ValidRequest());

        Assert.True(result.Authorized);
        Assert.Equal("abc-123", result.AuthorizationCode);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ThrowsBankUnavailableException_WhenBankReturnsServiceUnavailable()
    {
        var sut = CreateSut(HttpStatusCode.ServiceUnavailable, null);

        await Assert.ThrowsAsync<BankUnavailableException>(() => sut.ProcessPaymentAsync(ValidRequest()));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ThrowsBankUnavailableException_WhenBankReturnsBadRequest()
    {
        var sut = CreateSut(HttpStatusCode.BadRequest, new { error_message = "Not all required properties were sent in the request" });

        await Assert.ThrowsAsync<BankMalformedRequestException>(() => sut.ProcessPaymentAsync(ValidRequest()));
    }
}
