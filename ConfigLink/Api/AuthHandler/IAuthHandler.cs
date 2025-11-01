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
}