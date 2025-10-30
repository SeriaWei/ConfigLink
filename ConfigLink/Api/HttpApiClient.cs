using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace ConfigLink.Api
{
    /// <summary>
    /// HTTP API客户端，支持多种认证方式、重试和超时
    /// </summary>
    public class HttpApiClient : IHttpApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfig _config;
        private readonly IAuthHandler _authHandler;
        private bool _disposed = false;

        public HttpApiClient(ApiConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
            _authHandler = AuthHandlerFactory.Create(_config.Auth);
        }

        private void ConfigureHeaders()
        {
            if (_config.Headers == null) return;

            foreach (var header in _config.Headers)
            {
                // 跳过 Content-Type 等内容头部，这些会在 CreateContent 中处理
                if (IsContentHeader(header.Key))
                    continue;

                // 处理特殊占位符
                var value = ProcessPlaceholders(header.Value);
                
                // 移除现有的头部（如果存在）
                _httpClient.DefaultRequestHeaders.Remove(header.Key);
                
                // 添加新的头部
                _httpClient.DefaultRequestHeaders.Add(header.Key, value);
            }
        }

        private static bool IsContentHeader(string headerName)
        {
            return headerName.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Content-Disposition", StringComparison.OrdinalIgnoreCase);
        }

        private string ProcessPlaceholders(string value)
        {
            // 处理 {{guid}} 占位符
            if (value.Contains("{{guid}}"))
            {
                value = value.Replace("{{guid}}", Guid.NewGuid().ToString());
            }

            // 可以添加更多占位符处理
            return value;
        }

        private string ProcessDataPlaceholders(string template, object data)
        {
            // 处理形如 {PropertyName} 的占位符
            if (!template.Contains('{')) return template;

            var result = template;
            var dataDict = ConvertToStringDictionary(data);

            foreach (var kvp in dataDict)
            {
                var placeholder = $"{{{kvp.Key}}}";
                if (result.Contains(placeholder))
                {
                    result = result.Replace(placeholder, kvp.Value ?? string.Empty);
                }
            }

            return result;
        }

        private Dictionary<string, string?> ConvertToStringDictionary(object data)
        {
            var result = new Dictionary<string, string?>();

            if (data is Dictionary<string, object?> dict)
            {
                foreach (var kvp in dict)
                {
                    result[kvp.Key] = kvp.Value?.ToString();
                }
            }
            else
            {
                // 使用反射获取属性
                var properties = data.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(data);
                    result[prop.Name] = value?.ToString();
                }
            }

            return result;
        }

        private HttpContent CreateContent(object data)
        {
            if (data == null)
                return new StringContent(string.Empty, Encoding.UTF8, "application/json");

            string jsonData;
            if (data is string str)
            {
                jsonData = str;
            }
            else
            {
                jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
            }

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            
            // 设置其他内容头部
            if (_config.Headers != null)
            {
                foreach (var header in _config.Headers)
                {
                    if (IsContentHeader(header.Key))
                    {
                        var value = ProcessPlaceholders(header.Value);
                        
                        if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(value);
                        }
                        else
                        {
                            content.Headers.Remove(header.Key);
                            content.Headers.Add(header.Key, value);
                        }
                    }
                }
            }

            return content;
        }

        /// <summary>
        /// 发送数据到指定路径的API端点
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="path">API路径</param>
        /// <param name="method">HTTP方法，默认为POST</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API调用结果</returns>
        public async Task<ApiResult> SendAsync(object data, string path, string method = "POST", CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var maxRetries = Math.Max(0, _config.Retry);
            Exception? lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // 配置认证
                    var authConfigured = await _authHandler.ConfigureAuthAsync(_httpClient, _config);
                    if (!authConfigured)
                    {
                        return new ApiResult
                        {
                            Success = false,
                            ErrorMessage = "Authentication configuration failed",
                            Duration = stopwatch.Elapsed
                        };
                    }

                    // 配置头部
                    ConfigureHeaders();

                    // 构建完整URI（基础端点 + 指定路径）
                    var uri = BuildUriWithPath(data, path);

                    // 发送请求
                    var httpMethodUpper = method.ToUpper();
                    HttpResponseMessage? response = httpMethodUpper switch
                    {
                        "GET" => await _httpClient.GetAsync(uri, cancellationToken),
                        "POST" => await _httpClient.PostAsync(uri, CreateContent(data), cancellationToken),
                        "PUT" => await _httpClient.PutAsync(uri, CreateContent(data), cancellationToken),
                        "DELETE" => await _httpClient.DeleteAsync(uri, cancellationToken),
                        "PATCH" => await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, uri) 
                        { 
                            Content = CreateContent(data) 
                        }, cancellationToken),
                        _ => throw new NotSupportedException($"HTTP method {method} is not supported")
                    };

                    var responseContent = await response.Content.ReadAsStringAsync();

                    return new ApiResult
                    {
                        Success = response.IsSuccessStatusCode,
                        StatusCode = (int)response.StatusCode,
                        ResponseContent = responseContent,
                        Duration = stopwatch.Elapsed,
                        ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}: {response.ReasonPhrase}"
                    };
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    lastException = ex;
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(1000 * (attempt + 1), cancellationToken); // 指数退避
                        continue;
                    }

                    return new ApiResult
                    {
                        Success = false,
                        ErrorMessage = "Request timeout",
                        Exception = ex,
                        Duration = stopwatch.Elapsed
                    };
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(1000 * (attempt + 1), cancellationToken);
                        continue;
                    }

                    return new ApiResult
                    {
                        Success = false,
                        ErrorMessage = $"HTTP request failed: {ex.Message}",
                        Exception = ex,
                        Duration = stopwatch.Elapsed
                    };
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(1000 * (attempt + 1), cancellationToken);
                        continue;
                    }

                    return new ApiResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        Exception = ex,
                        Duration = stopwatch.Elapsed
                    };
                }
                finally
                {
                    // 这里不需要dispose response，因为HttpClient会处理
                }
            }

            return new ApiResult
            {
                Success = false,
                ErrorMessage = lastException?.Message ?? "Unknown error",
                Exception = lastException,
                Duration = stopwatch.Elapsed
            };
        }

        private string BuildUriWithPath(object data, string path)
        {
            // 获取基础端点URL
            var baseUri = _config.Endpoint.TrimEnd('/');
            
            // 组合路径
            var fullPath = path.StartsWith('/') ? path : '/' + path;
            var uri = baseUri + fullPath;

            return uri;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}