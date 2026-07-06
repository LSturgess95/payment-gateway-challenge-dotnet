using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Services
{
    public class PaymentsService
    {
        private readonly PaymentsRepository _paymentsRepository;

        public PaymentsService(PaymentsRepository paymentsRepository)
        {
            _paymentsRepository = paymentsRepository;
        }

        public async Task<ProcessPaymentResponse> Get(Guid id)
        {
            if (id == default)
                return null;

            var payment = _paymentsRepository.Get(id);

            return payment.ToResponse();
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest paymentRequest)
        {
            if (paymentRequest == null)
                return null;

            var payment = paymentRequest.ToDomain();

            // TODO async call to Authorise payment via bank simulator - will need to create an http client factory

            _paymentsRepository.Add(payment);

            return payment.ToResponse();
        }
    }
}
