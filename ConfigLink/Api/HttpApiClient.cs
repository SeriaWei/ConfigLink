using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace ConfigLink.Api
{
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
                if (IsContentHeader(header.Key))
                    continue;

                var value = ProcessPlaceholders(header.Value);

                _httpClient.DefaultRequestHeaders.Remove(header.Key);

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
            if (value.Contains("{{guid}}"))
            {
                value = value.Replace("{{guid}}", Guid.NewGuid().ToString());
            }

            return value;
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

        public async Task<ApiResult> SendAsync(object data, string path, string method = "POST", CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var maxRetries = Math.Max(0, _config.Retry);
            Exception? lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    ConfigureHeaders();

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

                    var uri = BuildUriWithPath(data, path);

                    var httpMethodUpper = method.ToUpper();

                    using HttpResponseMessage? response = httpMethodUpper switch
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
                        await Task.Delay(1000 * (attempt + 1), cancellationToken);
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
            var baseUri = _config.Endpoint.TrimEnd('/');

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