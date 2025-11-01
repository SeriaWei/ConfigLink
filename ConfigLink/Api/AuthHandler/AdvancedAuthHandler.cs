using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
                        ApplyAuthHeaders(httpClient, cachedAuth.TokenJson, config.Headers);
                        return true;
                    }
                }
            }

            // Execute the authentication request to get token
            var tokenJson = await ExecuteAuthRequestAsync(httpClient, config);
            if (tokenJson == null)
                return false;

            if (config.CacheToken)
            {
                lock (_lockObject)
                {
                    // Try to extract expiration from response if available
                    DateTime expiresAt = DateTime.UtcNow.AddMinutes(30); // Default cache for 30 minutes
                    
                    // Check if response has an expires_in field in the original response (under "response" key)
                    using var tempDoc = JsonDocument.Parse(tokenJson);
                    var rootElement = tempDoc.RootElement;
                    if (rootElement.TryGetProperty("response", out JsonElement responseElement))
                    {
                        if (responseElement.TryGetProperty("expires_in", out JsonElement expiresElement) && 
                            expiresElement.TryGetInt32(out int expiresIn))
                        {
                            expiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 300); // Subtract 5 minutes as buffer
                        }
                    }

                    _cache[cacheKey] = new AdvancedAuthCache
                    {
                        TokenJson = tokenJson,
                        ExpiresAt = expiresAt
                    };
                }
            }

            ApplyAuthHeaders(httpClient, tokenJson, config.Headers);
            return true;
        }

        private async Task<string?> ExecuteAuthRequestAsync(HttpClient httpClient, ApiConfig config)
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
                
                return jsonString;
            }
            catch (Exception)
            {
                // In a real scenario, we'd want to log this exception
                return null;
            }
        }



        private static void ApplyAuthHeaders(HttpClient httpClient, string tokenJson, Dictionary<string, string>? configHeaders)
        {
            // Clear any existing headers
            httpClient.DefaultRequestHeaders.Clear();

            // Apply headers from config, replacing {{response.*}} placeholders with actual values
            if (configHeaders != null)
            {
                foreach (var header in configHeaders)
                {
                    var headerValue = ReplacePlaceholders(header.Value, tokenJson);
                    httpClient.DefaultRequestHeaders.Add(header.Key, headerValue);
                }
            }
        }

        private static string ReplacePlaceholders(string template, string tokenJson)
        {
            var result = template;
            var regex = new System.Text.RegularExpressions.Regex(@"\{\{response\.([^}]+)\}\}");
            var matches = regex.Matches(template);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var fullMatch = match.Value; // e.g., {{response.auth.token}}
                var path = match.Groups[1].Value; // e.g., auth.token

                var value = GetValueFromPath(tokenJson, path);
                result = result.Replace(fullMatch, value ?? "");
            }

            return result;
        }

        private static string? GetValueFromPath(string tokenJson, string path)
        {
            try
            {
                using var doc = JsonDocument.Parse(tokenJson);
                var rootElement = doc.RootElement;
                
                // The root is structured as {"response": <original_response_data>}
                // So we need to first access the "response" property and then access the path within that
                if (!rootElement.TryGetProperty("response", out var responseElement))
                    return null;

                // Now extract the value using the path from the nested response data
                return GetNestedValue(responseElement, path);
            }
            catch
            {
                return null;
            }
        }

        private static string? GetNestedValue(JsonElement rootElement, string path)
        {
            var parts = path.Split('.');
            JsonElement currentElement = rootElement;

            foreach (var part in parts)
            {
                if (currentElement.ValueKind != JsonValueKind.Object)
                    return null;

                if (!currentElement.TryGetProperty(part, out currentElement))
                    return null;
            }

            return currentElement.ValueKind switch
            {
                JsonValueKind.String => currentElement.GetString(),
                JsonValueKind.Number => currentElement.TryGetInt64(out long l) ? l.ToString() : 
                                        currentElement.TryGetDouble(out double d) ? d.ToString() : 
                                        currentElement.GetDouble().ToString(),
                JsonValueKind.True => true.ToString(),
                JsonValueKind.False => false.ToString(),
                JsonValueKind.Null => null,
                JsonValueKind.Array => currentElement.GetArrayLength().ToString(),
                _ => currentElement.ToString()
            };
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