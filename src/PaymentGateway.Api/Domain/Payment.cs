namespace PaymentGateway.Api.Domain
{
    public class Payment
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
        public string? AuthorizationCode { get; set; }
        public required string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public required string CurrencyCode { get; set; }
        public int Amount { get; set; }
        public required string Cvv { get; set; }
    }
}
