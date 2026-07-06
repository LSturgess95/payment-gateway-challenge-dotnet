using System.Collections.Concurrent;

using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Repositories;

public class PaymentsRepository
{
    public ConcurrentDictionary<Guid, Payment> Payments = new();

    public bool Add(Payment payment)
    {
        return Payments.TryAdd(payment.Id, payment);
    }

    public Payment Get(Guid id)
    {
        Payments.TryGetValue(id, out Payment payment);
        return payment;
    }
}