using Microsoft.Extensions.Logging.Abstractions;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Tests.Unit.Repositories;

public class PaymentRepositoryTests
{
    private readonly PaymentRepository _sut = new(NullLogger<PaymentRepository>.Instance);

    private static Payment NewPayment(Guid id) => new()
    {
        Id = id,
        CardNumber = "1234123412341234",
        CurrencyCode = "GBP",
        Cvv = "123",
    };

    [Fact]
    public void Save_ReturnsTrue_AndStoresPayment_WhenIdIsNew()
    {
        var payment = NewPayment(Guid.NewGuid());

        var result = _sut.Save(payment);

        Assert.True(result);
        Assert.Same(payment, _sut.Get(payment.Id));
    }

    [Fact]
    public void Save_ReturnsFalse_WhenIdAlreadyExists()
    {
        var id = Guid.NewGuid();
        _sut.Save(NewPayment(id));

        var result = _sut.Save(NewPayment(id));

        Assert.False(result);
    }

    [Fact]
    public void Get_ReturnsNull_WhenPaymentDoesNotExist()
    {
        var result = _sut.Get(Guid.NewGuid());

        Assert.Null(result);
    }
}
