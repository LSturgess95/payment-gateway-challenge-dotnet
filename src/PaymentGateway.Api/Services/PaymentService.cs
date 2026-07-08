using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Clients;

namespace PaymentGateway.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private readonly IPaymentRepository _paymentsRepository;
        private readonly IBankClient _bankClient;

        public PaymentService(ILogger<PaymentService> logger, IPaymentRepository paymentsRepository, IBankClient bankClient)
        {
            _logger = logger;
            _paymentsRepository = paymentsRepository;
            _bankClient = bankClient;
        }

        public Payment Get(Guid id)
        {
            if (id == default)
            {
                _logger.LogWarning("id value has not been set");
                return null!;
            }

            var payment = _paymentsRepository.Get(id);

            return payment!;
        }

        public async Task<Payment> ProcessPaymentAsync(ProcessPaymentRequest paymentRequest)
        {
            var payment = paymentRequest.ToDomain();

            var bankResponse = await _bankClient.ProcessPaymentAsync(payment.ToBankClientRequest());

            if (bankResponse.Authorized)
            {
                payment.Status = PaymentStatus.Authorized;
                payment.AuthorizationCode = bankResponse.AuthorizationCode;
            } else
            {
                payment.Status = PaymentStatus.Declined;
            }

            _paymentsRepository.Save(payment);

            return payment;
        }
    }
}
