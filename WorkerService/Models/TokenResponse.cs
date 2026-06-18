using System.Text.Json.Serialization;

namespace WorkerService.Models
{
    /// <summary>
    /// Represents an OAuth2 token response from the OpenSky authentication endpoint.
    /// </summary>
    /// <remarks>
    /// The class maps the JSON returned by the token endpoint. AccessToken contains the
    /// bearer token to use in Authorization headers; ExpiresIn is the lifetime in seconds.
    /// </remarks>
    public class TokenResponse
    {
        /// <summary>
        /// The bearer access token returned by the authentication endpoint.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Token lifetime in seconds. Use this value to calculate a refresh time.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}