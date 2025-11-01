using System.Text.Json.Serialization;

namespace ConfigLink.Api
{
    /// <summary>
    /// API配置类，对应api.config.json中的单个平台配置
    /// </summary>
    public class ApiConfig
    {
        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        [JsonPropertyName("auth")]
        public string Auth { get; set; } = "none";

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("apiKey")]
        public string? ApiKey { get; set; }

        [JsonPropertyName("keyLocation")]
        public string? KeyLocation { get; set; }

        [JsonPropertyName("keyName")]
        public string? KeyName { get; set; }

        [JsonPropertyName("tokenUrl")]
        public string? TokenUrl { get; set; }

        [JsonPropertyName("clientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("clientSecret")]
        public string? ClientSecret { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("cacheToken")]
        public bool CacheToken { get; set; } = false;

        [JsonPropertyName("headers")]
        public Dictionary<string, string>? Headers { get; set; }

        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;

        [JsonPropertyName("retry")]
        public int Retry { get; set; } = 0;

        [JsonPropertyName("request")]
        public AdvancedAuthRequest? Request { get; set; }

    }

    public class AdvancedAuthRequest
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("method")]
        public string? Method { get; set; }

        [JsonPropertyName("headers")]
        public Dictionary<string, string>? Headers { get; set; }

        [JsonPropertyName("body")]
        public object? Body { get; set; }
    }

    /// <summary>
    /// API配置集合，包含多个平台的配置
    /// </summary>
    public class ApiConfigs : Dictionary<string, ApiConfig>
    {
        public static ApiConfigs LoadFromJson(string json)
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<ApiConfigs>(json);
            return result ?? new ApiConfigs();
        }
    }

    /// <summary>
    /// API调用结果
    /// </summary>
    public class ApiResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? ResponseContent { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// 认证类型枚举
    /// </summary>
    public enum AuthType
    {
        None,
        Basic,
        Bearer,
        ApiKey,
        OAuth2,
        Advanced
    }
}