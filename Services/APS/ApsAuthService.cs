using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace cloudmind_dwg_function.Services.APS
{
    public class ApsAuthService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _http;

        public ApsAuthService(IConfiguration config, IHttpClientFactory factory)
        {
            _http = factory.CreateClient();

            _clientId = config["APS_CLIENT_ID"] 
                ?? throw new Exception("APS_CLIENT_ID missing");

            _clientSecret = config["APS_CLIENT_SECRET"] 
                ?? throw new Exception("APS_CLIENT_SECRET missing");
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var dict = new Dictionary<string, string>
            {
                {"client_id", _clientId},
                {"client_secret", _clientSecret},
                {"grant_type", "client_credentials"},
                {"scope", "data:read data:write data:create bucket:create bucket:read"}
            };

            var response = await _http.PostAsync(
                "https://developer.api.autodesk.com/authentication/v2/token",
                new FormUrlEncodedContent(dict));

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("access_token").GetString()!;
        }
    }
}
