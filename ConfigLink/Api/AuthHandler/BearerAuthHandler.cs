using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConfigLink.Api
{
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
}