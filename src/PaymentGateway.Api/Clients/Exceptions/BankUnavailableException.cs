using System.Net;

namespace PaymentGateway.Api.Clients.Exceptions
{
    public class BankUnavailableException : Exception
    {
        public BankUnavailableException(HttpStatusCode statusCode)
        : base($"Bank returned {(int)statusCode} {statusCode}.") { }
    }
}
