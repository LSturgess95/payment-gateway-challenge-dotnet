using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Tests.Integration;

public class PaymentControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    // The API serializes with a camelCase naming policy (see Program.cs); match it here
    // so ReadFromJsonAsync doesn't silently deserialize every property to its default.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly WebApplicationFactory<Program> _factory;

    public PaymentControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static ProcessPaymentRequest ValidRequest() => new()
    {
        CardNumber = "1234123412341234",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        CurrencyCode = "GBP",
        Amount = 500,
        Cvv = "123",
    };

    private HttpClient CreateClientWithBank(IBankClient bankClient)
    {
        var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBankClient>();
                services.AddSingleton(bankClient);
            }));

        return factory.CreateClient();
    }

    [Fact]
    public async Task GetPayment_ReturnsNotFound_WhenPaymentDoesNotExist()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/payment/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPayment_ReturnsOk_WhenPaymentExists()
    {
        var repository = _factory.Services.GetRequiredService<IPaymentRepository>();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumber = "1234123412341234",
            ExpiryMonth = 12,
            ExpiryYear = 2027,
            CurrencyCode = "GBP",
            Amount = 500,
            Cvv = "123",
        };
        repository.Save(payment);

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/payment/{payment.Id}");
        var body = await response.Content.ReadFromJsonAsync<PaymentResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(payment.Id, body!.Id);
        Assert.Equal(payment.Status, body!.Status);
        Assert.Equal("1234", body.CardNumberLastFour);
        Assert.Equal(payment.ExpiryMonth, body!.ExpiryMonth);
        Assert.Equal(payment.ExpiryYear, body!.ExpiryYear);
        Assert.Equal(payment.CurrencyCode, body!.CurrencyCode);
        Assert.Equal(payment.Amount, body!.Amount);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsCreated_WithAuthorizedStatus_WhenBankAuthorizes()
    {
        var fakeBank = new FakeBankClient(new BankPaymentResponse { Authorized = true, AuthorizationCode = "auth-123" });
        var client = CreateClientWithBank(fakeBank);

        var response = await client.PostAsJsonAsync("/api/payment", ValidRequest(), JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<PaymentResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal(PaymentStatus.Authorized, body!.Status);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsCreated_WithDeclinedStatus_WhenBankDeclines()
    {
        var fakeBank = new FakeBankClient(new BankPaymentResponse { Authorized = false, AuthorizationCode = "" });
        var client = CreateClientWithBank(fakeBank);

        var response = await client.PostAsJsonAsync("/api/payment", ValidRequest(), JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<PaymentResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(PaymentStatus.Declined, body!.Status);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsBadRequest_WithRejectionReasons_WhenCardExpired()
    {
        var client = _factory.CreateClient();
        var request = ValidRequest();
        request.ExpiryMonth = 1;
        request.ExpiryYear = 2020;

        var response = await client.PostAsJsonAsync("/api/payment", request, JsonOptions);
        var body = await response.Content.ReadFromJsonAsync<PaymentResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(PaymentStatus.Rejected, body!.Status);
        Assert.Contains("Card expiry date must be in the future.", body.RejectionReasons!);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsBadGateway_WhenBankIsUnavailable()
    {
        var fakeBank = new FakeBankClient(new BankUnavailableException(HttpStatusCode.ServiceUnavailable));
        var client = CreateClientWithBank(fakeBank);

        var response = await client.PostAsJsonAsync("/api/payment", ValidRequest(), JsonOptions);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    private class FakeBankClient : IBankClient
    {
        private readonly BankPaymentResponse? _response;
        private readonly Exception? _exceptionToThrow;

        public FakeBankClient(BankPaymentResponse response) => _response = response;
        public FakeBankClient(Exception exceptionToThrow) => _exceptionToThrow = exceptionToThrow;

        public Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request)
        {
            if (_exceptionToThrow is not null)
                throw _exceptionToThrow;

            return Task.FromResult(_response!);
        }
    }
}
