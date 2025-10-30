using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConfigLink.Api
{
    /// <summary>
    /// 认证处理器接口
    /// </summary>
    public interface IAuthHandler
    {
        Task<bool> ConfigureAuthAsync(HttpClient httpClient, ApiConfig config);
    }

    /// <summary>
    /// 无认证处理器
    /// </summary>
    public class NoneAuthHandler : IAuthHandler
    {
        public Task<bool> ConfigureAuthAsync(HttpClient httpClient, ApiConfig config)
        {
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Basic认证处理器
    /// </summary>
    public class BasicAuthHandler : IAuthHandler
    {
        public Task<bool> ConfigureAuthAsync(HttpClient httpClient, ApiConfig config)
        {
            if (string.IsNullOrEmpty(config.Username) || string.IsNullOrEmpty(config.Password))
                return Task.FromResult(false);

            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Username}:{config.Password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Bearer Token认证处理器
    /// </summary>
    public class BearerAuthHandler : IAuthHandler
    {
        public Task<bool> ConfigureAuthAsync(HttpClient httpClient, ApiConfig config)
        {
            if (string.IsNullOrEmpty(config.Token))
                return Task.FromResult(false);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// API Key认证处理器
    /// </summary>
    public class ApiKeyAuthHandler : IAuthHandler
    {
        public Task<bool> ConfigureAuthAsync(HttpClient httpClient, ApiConfig config)
        {
            if (string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.KeyName))
                return Task.FromResult(false);

            var keyLocation = config.KeyLocation?.ToLower() ?? "header";
            
            if (keyLocation == "header")
            {
                httpClient.DefaultRequestHeaders.Add(config.KeyName, config.ApiKey);
            }
            // 对于query参数，在ApiClient中处理
            
            return Task.FromResult(true);
        }
    }

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
                var response = await httpClient.PostAsync(config.TokenUrl, content);

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
    }

    /// <summary>
    /// 认证处理器工厂
    /// </summary>
    public static class AuthHandlerFactory
    {
        public static IAuthHandler Create(string authType)
        {
            return authType.ToLower() switch
            {
                "basic" => new BasicAuthHandler(),
                "bearer" => new BearerAuthHandler(),
                "apikey" => new ApiKeyAuthHandler(),
                "oauth2" => new OAuth2AuthHandler(),
                _ => new NoneAuthHandler()
            };
        }
    }
}