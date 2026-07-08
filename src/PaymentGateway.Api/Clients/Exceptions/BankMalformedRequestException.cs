using System.Net;

namespace PaymentGateway.Api.Clients.Exceptions
{
    public class BankMalformedRequestException : Exception
    {
        public BankMalformedRequestException(HttpStatusCode statusCode)
        : base($"Bank returned {(int)statusCode} {statusCode}.") { }
    }
}
