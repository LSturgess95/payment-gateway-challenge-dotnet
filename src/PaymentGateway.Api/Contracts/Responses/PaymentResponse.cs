using System.Text.Json.Serialization;

using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Contracts.Responses;

public class PaymentResponse
{
    public Guid? Id { get; set; }
    public PaymentStatus? Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? RejectionReasons { get; set; }
    public string? CardNumberLastFour { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public string? CurrencyCode { get; set; }
    public int? Amount { get; set; }
}