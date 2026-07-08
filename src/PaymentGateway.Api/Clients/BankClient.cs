using System.Net;

using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Clients.Exceptions;

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
                _logger.BankProcessPaymentKnownFailure(response.StatusCode);

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    throw new BankUnavailableException(response.StatusCode);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BankMalformedRequestException(response.StatusCode);
                }
                else
                {
                    _logger.BankProcessPaymentUnknownFailure(response.StatusCode, await response.Content.ReadAsStringAsync());
                    throw new Exception($"Bank returned an unknown error response {response.StatusCode}");
                }
            }

            var responseContent = await response.Content.ReadFromJsonAsync<BankPaymentResponse>();
            return responseContent is null ? throw new BankEmptyResponseContentException() : responseContent;
        }
    }
}
