using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConfigLink.Api
{
    /// <summary>
    /// OAuth2认证处理器
    /// </summary>
    public class OAuth2AuthHandler : IAuthHandler
    {
        private static readonly Dictionary<string, OAuthToken> _tokenCache = new();
        private static readonly object _lockObject = new();

        public async Task<bool> ConfigureAuthAsync(HttpClient httpClient, ApiConfig config)
        {
            if (string.IsNullOrEmpty(config.TokenUrl) || 
                string.IsNullOrEmpty(config.ClientId) || 
                string.IsNullOrEmpty(config.ClientSecret))
                return false;

            var cacheKey = $"{config.TokenUrl}:{config.ClientId}";
            OAuthToken? token = null;

            if (config.CacheToken)
            {
                lock (_lockObject)
                {
                    if (_tokenCache.TryGetValue(cacheKey, out token) && !token.IsExpired)
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                        return true;
                    }
                }
            }

            // 获取新的token
            token = await GetOAuthTokenAsync(httpClient, config);
            if (token == null)
                return false;

            if (config.CacheToken)
            {
                lock (_lockObject)
                {
                    _tokenCache[cacheKey] = token;
                }
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            return true;
        }

        private async Task<OAuthToken?> GetOAuthTokenAsync(HttpClient httpClient, ApiConfig config)
        {
            try
            {
                var tokenRequest = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "client_credentials"),
                    new("client_id", config.ClientId!),
                    new("client_secret", config.ClientSecret!)
                };

                if (!string.IsNullOrEmpty(config.Scope))
                {
                    tokenRequest.Add(new("scope", config.Scope));
                }

                var content = new FormUrlEncodedContent(tokenRequest);
                
                // Handle relative tokenUrl by combining with endpoint if needed
                var tokenUrl = ResolveUrl(config.TokenUrl, config.Endpoint);
                var response = await httpClient.PostAsync(tokenUrl, content);

                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(jsonResponse);

                if (tokenResponse?.access_token == null)
                    return null;

                return new OAuthToken
                {
                    AccessToken = tokenResponse.access_token,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in ?? 3600)
                };
            }
            catch
            {
                return null;
            }
        }

        private class OAuthTokenResponse
        {
            public string? access_token { get; set; }
            public int? expires_in { get; set; }
            public string? token_type { get; set; }
        }

        private class OAuthToken
        {
            public string AccessToken { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public bool IsExpired => DateTime.UtcNow >= ExpiresAt.AddMinutes(-5); // 提前5分钟刷新
        }

        private static string ResolveUrl(string? tokenUrl, string? endpoint)
        {
            if (string.IsNullOrEmpty(tokenUrl))
                return string.Empty;

            if (Uri.TryCreate(tokenUrl, UriKind.Absolute, out _))
                return tokenUrl;

            if (!string.IsNullOrEmpty(endpoint))
            {
                var baseUri = new Uri(endpoint);
                var combinedUri = new Uri(baseUri, tokenUrl);
                return combinedUri.ToString();
            }

            return tokenUrl;
        }
    }
}