using System.Text.Json;
using Shared.Interfaces;
using Shared.Dtos;

namespace Services;

public class TokenTestService : ITokenTestService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenTestService> _logger;

    public TokenTestService(HttpClient httpClient, IConfiguration configuration, ILogger<TokenTestService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenTestResponse?> GetClientCredentialsTokenAsync()
    {
        try
        {
            var oktaDomain = _configuration["Okta:OktaDomain"];
            var clientId = _configuration["Okta:ClientId"];
            var clientSecret = _configuration["Okta:ClientSecret"];
            var authServerId = _configuration["Okta:AuthorizationServerId"] ?? "default";

            if (string.IsNullOrEmpty(oktaDomain) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Missing Okta configuration: OktaDomain={OktaDomain}, ClientId={ClientId}, HasClientSecret={HasSecret}",
                    oktaDomain, clientId, !string.IsNullOrEmpty(clientSecret));
                return null;
            }

            var tokenEndpoint = $"{oktaDomain}/oauth2/{authServerId}/v1/token";

            var requestData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", clientId!),
                new("client_secret", clientSecret!),
                new("scope", "api://default")
            };

            var requestContent = new FormUrlEncodedContent(requestData);

            _logger.LogInformation("Requesting client credentials token from: {TokenEndpoint} with ClientId: {ClientId}",
                tokenEndpoint, clientId);

            var response = await _httpClient.PostAsync(tokenEndpoint, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Raw JSON response from Okta:\n{ResponseContent}", responseContent);

                var tokenResponse = JsonSerializer.Deserialize<TokenTestResponse>(responseContent);
                return tokenResponse;
            }
            else
            {
                _logger.LogError("Failed to get client credentials token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client credentials token");
            return null;
        }
    }
}
