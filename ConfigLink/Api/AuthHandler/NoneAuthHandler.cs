using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConfigLink.Api
{
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
}