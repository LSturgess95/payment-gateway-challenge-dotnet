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
    }
}
