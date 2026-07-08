using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Clients.Exceptions;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Unit.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _repository = new();
    private readonly Mock<IBankClient> _bankClient = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _repository.Setup(r => r.Save(It.IsAny<Payment>())).Returns(true);
        _sut = new PaymentService(NullLogger<PaymentService>.Instance, _repository.Object, _bankClient.Object);
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

    [Fact]
    public void Get_ReturnsNull_WhenIdIsDefault()
    {
        var result = _sut.Get(Guid.Empty);

        Assert.Null(result);
    }

    [Fact]
    public void Get_ReturnsPayment_WhenFoundInRepository()
    {
        var id = Guid.NewGuid();
        var payment = new Payment { Id = id, CardNumber = "1234123412341234", CurrencyCode = "GBP", Cvv = "123" };
        _repository.Setup(r => r.Get(id)).Returns(payment);

        var result = _sut.Get(id);

        Assert.Same(payment, result);
    }

    [Fact]
    public void Get_ReturnsNull_WhenNotFoundInRepository()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.Get(id)).Returns((Payment?)null);

        var result = _sut.Get(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessPaymentAsync_SetsAuthorizedStatusAndAuthorizationCode_WhenBankAuthorizes()
    {
        _bankClient
            .Setup(c => c.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankPaymentResponse { Authorized = true, AuthorizationCode = "auth-code-123" });

        var result = await _sut.ProcessPaymentAsync(ValidRequest());

        Assert.Equal(PaymentStatus.Authorized, result.Status);
        Assert.Equal("auth-code-123", result.AuthorizationCode);
        _repository.Verify(r => r.Save(result), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_SetsDeclinedStatus_WhenBankDeclines()
    {
        _bankClient
            .Setup(c => c.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankPaymentResponse { Authorized = false, AuthorizationCode = "" });

        var result = await _sut.ProcessPaymentAsync(ValidRequest());

        Assert.Equal(PaymentStatus.Declined, result.Status);
        Assert.Null(result.AuthorizationCode);
        _repository.Verify(r => r.Save(result), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_PropagatesBankUnavailableException_WithoutSaving()
    {
        _bankClient
            .Setup(c => c.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>()))
            .ThrowsAsync(new BankUnavailableException(System.Net.HttpStatusCode.ServiceUnavailable));

        await Assert.ThrowsAsync<BankUnavailableException>(() => _sut.ProcessPaymentAsync(ValidRequest()));

        _repository.Verify(r => r.Save(It.IsAny<Payment>()), Times.Never);
    }
}
