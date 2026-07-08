using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    None = 0,
    Authorized,
    Declined,
    Rejected
}