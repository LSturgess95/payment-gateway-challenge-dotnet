using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Repositories
{
    public interface IPaymentRepository
    {
        bool Save(Payment payment);
        Payment? Get(Guid id);
    }
}
