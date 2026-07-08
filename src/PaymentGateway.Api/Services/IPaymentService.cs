using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Services
{
    public interface IPaymentService
    {
        Payment Get(Guid id);
        Task<Payment> ProcessPaymentAsync(ProcessPaymentRequest paymentRequest);
    }
}
