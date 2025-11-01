using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConfigLink.Api
{
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
}