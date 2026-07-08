using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;

namespace PaymentGateway.Api.Clients
{
    public interface IBankClient
    {
        Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request);
    }
}
