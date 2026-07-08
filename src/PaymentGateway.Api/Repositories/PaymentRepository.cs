using System.Collections.Concurrent;

using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(ILogger<PaymentRepository> logger)
    {
        _logger = logger;
    }

    public bool Save(Payment payment)
    {
        if (!_payments.TryAdd(payment.Id, payment))
        {
            _logger.PaymentSaveFailed(payment.Id);
            return false;
        }

        return true;
    }

    public Payment? Get(Guid id)
    {
        if (!_payments.TryGetValue(id, out var payment))
        {
            _logger.PaymentNotFound(id);
            return null;
        }

        return payment;
    }
}