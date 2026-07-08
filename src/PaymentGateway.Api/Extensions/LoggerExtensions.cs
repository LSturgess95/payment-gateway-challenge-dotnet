using System.Net;

namespace PaymentGateway.Api.Extensions
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Warning,
            Message = "Failed to retrieve payment with Id {paymentId}")]
        public static partial void PaymentNotFound(this ILogger logger, Guid paymentId);

        [LoggerMessage(
            EventId = 1002,
            Level = LogLevel.Error,
            Message = "Failed to save payment with Id {paymentId}")]
        public static partial void PaymentSaveFailed(this ILogger logger, Guid paymentId);

        [LoggerMessage(
            EventId = 1003,
            Level = LogLevel.Warning,
            Message = "Bank returned a known error {statusCode}")]
        public static partial void BankProcessPaymentKnownFailure(this ILogger logger, HttpStatusCode statusCode);

        [LoggerMessage(
            EventId = 1004,
            Level = LogLevel.Error,
            Message = "Bank returned an unknown error {statusCode} : {error}")]
        public static partial void BankProcessPaymentUnknownFailure(this ILogger logger, HttpStatusCode statusCode, string error);
    }
}
