using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Mappers
{
    public static class PaymentMapper
    {
        public static Payment ToDomain(this ProcessPaymentRequest request)
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.None,
                CardNumber = request.CardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount,
            };
        }
        public static ProcessPaymentResponse ToResponse(this Payment request)
        {
            return new ProcessPaymentResponse
            {
                Id = request.Id,
                Status = request.Status,
                CardNumberLastFour = request.CardNumber.Substring(request.CardNumber.Length - 4), //TODO add guardrails for < 4 length
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount,
            };
        }
    }
}
