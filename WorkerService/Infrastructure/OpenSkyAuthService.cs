using System.Text.Json;
using WorkerService.Models;

namespace WorkerService.Infrastructure
{
    public class OpenSkyAuthService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private string? _cachedToken;
        private DateTime _expiry;

        public OpenSkyAuthService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_cachedToken != null && DateTime.UtcNow < _expiry)
                return _cachedToken;

            // Use the factory instead of creating a new HttpClient per request
            using var client = _httpClientFactory.CreateClient();

            // Send the request to the AUTH endpoint, not the BASE API endpoint
            var request = new HttpRequestMessage(HttpMethod.Post, _config["OpenSky:AuthUrl"]);

            var body = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _config["OpenSky:clientId"] },
                { "client_secret", _config["OpenSky:clientSecret"] }
            };

            request.Content = new FormUrlEncodedContent(body);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            _cachedToken = tokenResponse.AccessToken;
            _expiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30); // refresh a bit early

            return _cachedToken!;
        }
    }
}