using System.Net;

namespace PaymentGateway.Api.Domain
{
    public class PaymentSaveFailureException : Exception
    {
        public PaymentSaveFailureException()
        : base("Failed to save payment") { }
    }
}
