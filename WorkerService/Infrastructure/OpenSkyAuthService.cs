using System.Text.Json;
using WorkerService.Models;

namespace WorkerService.Infrastructure
{
    /// <summary>
    /// Handles acquiring and caching OAuth2 tokens for calling the OpenSky API.
    /// </summary>
    /// <remarks>
    /// This service uses IHttpClientFactory to create short-lived HttpClient instances
    /// and caches the retrieved access token until shortly before expiry to avoid
    /// requesting tokens on every call.
    /// </remarks>
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

        /// <summary>
        /// Retrieve a valid access token. Uses a cached token when available and not expired.
        /// </summary>
        /// <returns>A bearer token string to be used in an Authorization header.</returns>
        /// <remarks>
        /// The cached token is refreshed slightly before its expiry (30 seconds) to prevent
        /// edge-case expiration during API calls. Callers should reuse this value until it
        /// expires instead of requesting tokens for every request.
        /// </remarks>
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