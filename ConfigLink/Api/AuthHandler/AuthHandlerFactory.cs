using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConfigLink.Api
{
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