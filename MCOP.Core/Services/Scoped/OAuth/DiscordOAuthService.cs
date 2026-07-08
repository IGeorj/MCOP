using DSharpPlus.Entities;
using MCOP.Utils;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;

namespace MCOP.Core.Services.Scoped.OAuth
{
    public interface IDiscordOAuthService
    {
        public Task<DiscordTokenResponse?> ExchangeCodeAsync(string code);
        public Task<DiscordTokenResponse?> RefreshTokenAsync(string refreshToken);
        public Task<DiscordUser?> FetchDiscordUserAsync(string accessToken);
    }

    public sealed class DiscordOAuthService : IDiscordOAuthService
    {
        private readonly ConfigurationService _config;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public DiscordOAuthService(
            ConfigurationService config, 
            ILogger logger, 
            IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("discord-oauth");
        }

        public async Task<DiscordTokenResponse?> ExchangeCodeAsync(string code)
        {
            var values = new Dictionary<string, string>
            {
                ["client_id"] = _config.CurrentConfiguration.DiscordOAuthConfig.ClientId,
                ["client_secret"] = _config.CurrentConfiguration.DiscordOAuthConfig.ClientSecret,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = _config.CurrentConfiguration.DiscordOAuthConfig.RedirectUri
            };

            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("oauth2/token", content);
            var payload = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("Discord Token Exchange Failed: {0}", payload);
                return null;
            }

            var tokenResponse = JsonConvert.DeserializeObject<DiscordTokenResponse>(payload);
            if (tokenResponse != null)
                tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            return tokenResponse;
        }

        public async Task<DiscordTokenResponse?> RefreshTokenAsync(string refreshToken)
        {
            var values = new Dictionary<string, string>
            {
                ["client_id"] = _config.CurrentConfiguration.DiscordOAuthConfig.ClientId,
                ["client_secret"] = _config.CurrentConfiguration.DiscordOAuthConfig.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };

            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("oauth2/token", content);
            var payload = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("Discord Token Refresh Failed: {0}", payload);
                return null;
            }

            var tokenResponse = JsonConvert.DeserializeObject<DiscordTokenResponse>(payload);
            if (tokenResponse != null)
            {
                tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            }

            return tokenResponse;
        }

        public async Task<DiscordUser?> FetchDiscordUserAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("users/@me");
            var payload = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("Fetch Discord User failed: {0}", payload);
                return null;
            }
            return JsonConvert.DeserializeObject<DiscordUser>(payload);
        }
    }
}
