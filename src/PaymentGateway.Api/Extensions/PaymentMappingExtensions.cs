using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Extensions
{
    public static class PaymentMappingExtensions
    {
        public static Payment ToDomain(this ProcessPaymentRequest request)
        {
            return new Payment
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.None,
                CardNumber = request.CardNumber!,
                ExpiryMonth = request.ExpiryMonth!.Value,
                ExpiryYear = request.ExpiryYear!.Value,
                CurrencyCode = request.CurrencyCode!,
                Amount = request.Amount!.Value,
                Cvv = request.Cvv!,
            };
        }

        public static PaymentResponse ToRejectedResponse(this ProcessPaymentRequest? paymentRequest, IEnumerable<string> errors)
        {
            return new PaymentResponse
            {
                Status = PaymentStatus.Rejected,
                RejectionReasons = errors.ToList(),
                CardNumberLastFour = string.IsNullOrWhiteSpace(paymentRequest?.CardNumber) ? "" : paymentRequest.CardNumber[^Math.Min(4, paymentRequest.CardNumber.Length)..],
                ExpiryMonth = paymentRequest?.ExpiryMonth ?? 0,
                ExpiryYear = paymentRequest?.ExpiryYear ?? 0,
                CurrencyCode = paymentRequest?.CurrencyCode,
                Amount = paymentRequest?.Amount ?? 0,
            };
        }

        public static PaymentResponse ToResponse(this Payment payment)
        {
            return new PaymentResponse
            {
                Id = payment?.Id,
                Status = payment?.Status,
                CardNumberLastFour = string.IsNullOrWhiteSpace(payment?.CardNumber) ? "" : payment.CardNumber[^Math.Min(4, payment.CardNumber.Length)..],
                ExpiryMonth = payment?.ExpiryMonth,
                ExpiryYear = payment?.ExpiryYear,
                CurrencyCode = payment?.CurrencyCode,
                Amount = payment?.Amount,
            };
        }

        public static BankPaymentRequest ToBankClientRequest(this Payment payment)
        {
            return new BankPaymentRequest
            {
                CardNumber = payment.CardNumber!,
                ExpiryDate = $"{payment.ExpiryMonth:00}/{payment.ExpiryYear}",
                Currency = payment.CurrencyCode!,
                Amount = payment.Amount,
                Cvv = payment.Cvv!,
            };
        }
    }
}
