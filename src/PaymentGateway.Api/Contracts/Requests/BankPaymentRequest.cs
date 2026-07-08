using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Contracts.Requests
{
    public class BankPaymentRequest
    {
        [JsonPropertyName("card_number")]
        public required string CardNumber { get; set; }

        [JsonPropertyName("expiry_date")]
        public required string ExpiryDate { get; set; } // "MM/YYYY"

        [JsonPropertyName("currency")]
        public required string Currency { get; set; } // ISO

        [JsonPropertyName("amount")]
        public required int Amount { get; set; }

        [JsonPropertyName("cvv")]
        public required string Cvv { get; set; }
    }
}
