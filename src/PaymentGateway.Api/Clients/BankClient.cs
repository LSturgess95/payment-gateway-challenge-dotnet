using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Clients
{
    public class BankClient : IBankClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BankClient> _logger;
        private readonly string _paymentsEndpoint = "/payments";

        public BankClient(HttpClient httpClient, ILogger<BankClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(_paymentsEndpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bank simulator returned {StatusCode}", response.StatusCode);
                throw new BankUnavailableException(response.StatusCode);
            }

            return (await response.Content.ReadFromJsonAsync<BankPaymentResponse>())!;
        }
    }
}
