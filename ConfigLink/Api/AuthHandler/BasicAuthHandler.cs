using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConfigLink.Api
{
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
}