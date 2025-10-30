namespace ConfigLink.Api
{
    /// <summary>
    /// HTTP API客户端工厂接口
    /// </summary>
    public interface IHttpApiClientFactory
    {
        /// <summary>
        /// 创建HTTP API客户端
        /// </summary>
        /// <param name="config">API配置</param>
        /// <returns>HTTP API客户端实例</returns>
        IHttpApiClient CreateClient(ApiConfig config);
    }

    /// <summary>
    /// 默认的HTTP API客户端工厂实现
    /// </summary>
    public class HttpApiClientFactory : IHttpApiClientFactory
    {
        public IHttpApiClient CreateClient(ApiConfig config)
        {
            return new HttpApiClient(config);
        }
    }
}