using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigLink.Api
{
    /// <summary>
    /// HTTP API客户端接口
    /// </summary>
    public interface IHttpApiClient : IDisposable
    {
        /// <summary>
        /// 发送数据到指定路径的API端点
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="path">API路径</param>
        /// <param name="method">HTTP方法，默认为POST</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API调用结果</returns>
        Task<ApiResult> SendAsync(object data, string path, string method = "POST", CancellationToken cancellationToken = default);
    }
}