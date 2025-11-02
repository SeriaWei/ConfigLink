using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ConfigLink.Extensions;

namespace ConfigLink.Api
{
    /// <summary>
    /// 高级认证处理器 - 用于处理自定义认证流程
    /// </summary>
    public class AdvancedAuthHandler : IAuthHandler
    {
        private static readonly Dictionary<string, AdvancedAuthCache> _cache = new();
        private static readonly object _lockObject = new();

        public async Task<bool> ConfigureAuthAsync(HttpClient httpClient, ApiConfig config)
        {
            if (config.Request == null)
                return false;

            var cacheKey = GenerateCacheKey(config);
            AdvancedAuthCache? cachedAuth = null;

            // Check if we have a valid cached token
            if (config.CacheToken)
            {
                lock (_lockObject)
                {
                    if (_cache.TryGetValue(cacheKey, out cachedAuth) && !cachedAuth.IsExpired)
                    {
                        // Parse cached token string to JsonDocument once for processing
                        using var cachedTokenDoc = JsonDocument.Parse(cachedAuth.TokenJson);
                        ApplyAuthHeaders(httpClient, cachedTokenDoc, config.Headers);
                        return true;
                    }
                }
            }

            // Execute the authentication request to get token
            using var tokenDoc = await ExecuteAuthRequestAsync(httpClient, config);
            if (tokenDoc == null)
                return false;

            if (config.CacheToken)
            {
                lock (_lockObject)
                {
                    // Try to extract expiration from response if available
                    DateTime expiresAt = DateTime.UtcNow.AddMinutes(30); // Default cache for 30 minutes
                    
                    var rootElement = tokenDoc.RootElement;
                    if (rootElement.TryGetProperty("response", out JsonElement responseElement))
                    {
                        if (responseElement.TryGetProperty("expires_in", out JsonElement expiresElement) && 
                            expiresElement.TryGetInt32(out int expiresIn))
                        {
                            expiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 300); // Subtract 5 minutes as buffer
                        }
                    }

                    // Store the JSON string in the cache for persistence (like before)
                    _cache[cacheKey] = new AdvancedAuthCache
                    {
                        TokenJson = tokenDoc.RootElement.ToString(), // Convert to JSON string representation
                        ExpiresAt = expiresAt
                    };
                }
            }

            ApplyAuthHeaders(httpClient, tokenDoc, config.Headers);
            return true;
        }

        private async Task<JsonDocument?> ExecuteAuthRequestAsync(HttpClient httpClient, ApiConfig config)
        {
            try
            {
                var authRequest = config.Request;
                if (authRequest == null)
                    return null;

                // Build the full URL from the auth request URL
                var requestUrl = ResolveUrl(authRequest.Url, config.Endpoint);
                if (string.IsNullOrEmpty(requestUrl))
                    return null;

                // Prepare the HTTP request
                var httpRequest = new HttpRequestMessage()
                {
                    Method = new HttpMethod(authRequest.Method ?? "POST"), // Use POST as default for auth requests
                    RequestUri = new Uri(requestUrl)
                };

                // Add headers from the request config
                if (authRequest.Headers != null)
                {
                    foreach (var header in authRequest.Headers)
                    {
                        // Content-Type and other content-specific headers need special handling
                        if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                        {
                            if (httpRequest.Content != null)
                            {
                                // Set content headers appropriately
                                httpRequest.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(header.Value);
                            }
                        }
                        else
                        {
                            // Safely add headers to the request, avoiding duplicate header exceptions
                            if (httpRequest.Headers.TryGetValues(header.Key, out var existingValues))
                            {
                                httpRequest.Headers.Remove(header.Key);
                            }
                            httpRequest.Headers.Add(header.Key, header.Value);
                        }
                    }
                }

                // Add body if present
                if (authRequest.Body != null)
                {
                    var jsonBody = JsonSerializer.Serialize(authRequest.Body);
                    httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                // Make the authentication request
                using var response = await httpClient.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                    return null;

                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Validate that response content is valid JSON
                using var originalDoc = JsonDocument.Parse(responseContent);
                var originalElement = originalDoc.RootElement;
                
                // Create a wrapper object that contains the response under a "response" key
                // Using proper JSON writing to avoid formatting issues
                using var responseStream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(responseStream, new JsonWriterOptions { Indented = false }))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("response");
                    originalElement.WriteTo(writer);
                    writer.WriteEndObject();
                }
                
                var jsonString = Encoding.UTF8.GetString(responseStream.ToArray());
                
                return JsonDocument.Parse(jsonString);
            }
            catch (Exception)
            {
                // In a real scenario, we'd want to log this exception
                return null;
            }
        }



        private static void ApplyAuthHeaders(HttpClient httpClient, JsonDocument tokenDoc, Dictionary<string, string>? configHeaders)
        {
            // Clear any existing headers
            httpClient.DefaultRequestHeaders.Clear();

            // Apply headers from config, replacing {{response.*}} placeholders with actual values
            if (configHeaders != null)
            {
                foreach (var header in configHeaders)
                {
                    var headerValue = ReplacePlaceholders(header.Value, tokenDoc);
                    httpClient.DefaultRequestHeaders.Add(header.Key, headerValue);
                }
            }
        }

        private static string ReplacePlaceholders(string template, JsonDocument tokenDoc)
        {
            var result = template;
            // Updated regex to handle any JSON path, not just "response" and also handle whitespace in placeholders
            var regex = new System.Text.RegularExpressions.Regex(@"\{\{\s*([^}]+?)\s*\}\}");
            var matches = regex.Matches(template);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var fullMatch = match.Value; // e.g., {{ response.auth.token }} or {{ response.token }}
                var path = match.Groups[1].Value.Trim(); // e.g., response.auth.token or response.token

                var value = tokenDoc.RootElement.GetValueByPath(path);
                result = result.Replace(fullMatch, value ?? "");
            }

            return result;
        }





        private static string GenerateCacheKey(ApiConfig config)
        {
            // Create a unique cache key based on endpoint and request details
            var key = $"{config.Endpoint}:{config.Request?.Url}:{JsonSerializer.Serialize(config.Request?.Body)}";
            return key.GetHashCode().ToString();
        }

        private static string ResolveUrl(string? requestUrl, string? endpoint)
        {
            if (string.IsNullOrEmpty(requestUrl))
                return string.Empty;

            // If requestUrl is already an absolute URL, return as-is
            if (Uri.TryCreate(requestUrl, UriKind.Absolute, out _))
                return requestUrl;

            // If requestUrl is relative and we have an endpoint, combine them
            if (!string.IsNullOrEmpty(endpoint))
            {
                var baseUri = new Uri(endpoint);
                var combinedUri = new Uri(baseUri, requestUrl);
                return combinedUri.ToString();
            }

            // If no endpoint is available, return as-is (likely will fail)
            return requestUrl;
        }

        private class AdvancedAuthCache
        {
            public string TokenJson { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        }
    }
}