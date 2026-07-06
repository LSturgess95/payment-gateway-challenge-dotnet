namespace PaymentGateway.Api.Models;

public enum PaymentStatus
{
    None = 0,
    Authorized,
    Declined,
    Rejected
}