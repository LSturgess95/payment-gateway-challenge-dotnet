using System.Net;

namespace PaymentGateway.Api.Clients.Exceptions
{
    public class BankEmptyResponseContentException : Exception
    {
        public BankEmptyResponseContentException()
        : base("Bank returned a malformed or empty payload.") { }
    }
}
