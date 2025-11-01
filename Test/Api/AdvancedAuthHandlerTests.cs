using System.Text;
using System.Text.Json;
using ConfigLink.Api;

namespace Test.Api
{
    public class AdvancedAuthHandlerTests : IDisposable
    {
        private HttpMessageHandlerStub _httpHandlerStub;
        private HttpClient _httpClient;

        public AdvancedAuthHandlerTests()
        {
            _httpHandlerStub = new HttpMessageHandlerStub();
            _httpClient = new HttpClient(_httpHandlerStub);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        [Fact]
        public async Task ConfigureAuthAsync_ValidAuthRequest_SetsAuthHeaders()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();
            var jsonResponse = JsonSerializer.Serialize(new { oauth_token = "test-token-value" });
            
            _httpHandlerStub.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.example.com/",
                CacheToken = false,
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer {{response.oauth_token}}" }
                },
                Request = new AdvancedAuthRequest
                {
                    Url = "/login",
                    Method = "POST",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    },
                    Body = new { username = "admin", password = "password" }
                }
            };

            // Act
            var result = await authHandler.ConfigureAuthAsync(_httpClient, config);

            // Assert
            Assert.True(result);
            Assert.True(_httpClient.DefaultRequestHeaders.Contains("Authorization"));
            var authHeader = _httpClient.DefaultRequestHeaders.GetValues("Authorization").First();
            Assert.Equal("Bearer test-token-value", authHeader);
        }

        [Fact]
        public async Task ConfigureAuthAsync_NestedResponseValue_ExtractsValueCorrectly()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();
            var jsonResponse = JsonSerializer.Serialize(new { auth = new { token = "nested-token-value" } });
            
            _httpHandlerStub.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.example.com/",
                CacheToken = false,
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer {{response.auth.token}}" }
                },
                Request = new AdvancedAuthRequest
                {
                    Url = "/auth",
                    Method = "POST",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    },
                    Body = new { username = "admin", password = "password" }
                }
            };

            // Act
            var result = await authHandler.ConfigureAuthAsync(_httpClient, config);

            // Assert
            Assert.True(result);
            Assert.True(_httpClient.DefaultRequestHeaders.Contains("Authorization"));
            var authHeader = _httpClient.DefaultRequestHeaders.GetValues("Authorization").First();
            Assert.Equal("Bearer nested-token-value", authHeader);
        }

        [Fact]
        public async Task ConfigureAuthAsync_InvalidResponse_ReturnsFalse()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();
            
            _httpHandlerStub.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);

            var config = new ApiConfig
            {
                Endpoint = "https://api.example.com/",
                CacheToken = false,
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer {{response.token}}" }
                },
                Request = new AdvancedAuthRequest
                {
                    Url = "/login",
                    Method = "POST",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    },
                    Body = new { username = "admin", password = "password" }
                }
            };

            // Act
            var result = await authHandler.ConfigureAuthAsync(_httpClient, config);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ConfigureAuthAsync_MissingRequest_ReturnsFalse()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();

            var config = new ApiConfig
            {
                Endpoint = "https://api.example.com/",
                CacheToken = false,
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer {{response.token}}" }
                },
                // Request is null
            };

            // Act
            var result = await authHandler.ConfigureAuthAsync(_httpClient, config);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ConfigureAuthAsync_RelativeTokenUrl_ResolvesCorrectly()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();
            var jsonResponse = JsonSerializer.Serialize(new { access_token = "relative-url-token" });
            
            _httpHandlerStub.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.example.com/",
                CacheToken = false,
                Headers = new Dictionary<string, string>
                {
                    { "X-API-Key", "{{response.access_token}}" }
                },
                Request = new AdvancedAuthRequest
                {
                    Url = "/token", // Relative URL
                    Method = "POST",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    },
                    Body = new { grant_type = "client_credentials" }
                }
            };

            // Act
            var result = await authHandler.ConfigureAuthAsync(_httpClient, config);

            // Assert
            Assert.True(result);
            Assert.True(_httpClient.DefaultRequestHeaders.Contains("X-API-Key"));
            var apiKeyHeader = _httpClient.DefaultRequestHeaders.GetValues("X-API-Key").First();
            Assert.Equal("relative-url-token", apiKeyHeader);
            
            // Verify the request was made to the correct resolved URL
            Assert.True(_httpHandlerStub.RequestUri?.ToString().EndsWith("/token"));
        }

        [Fact]
        public async Task ConfigureAuthAsync_CachedToken_UsesCachedValue()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();
            var jsonResponse = JsonSerializer.Serialize(new { token = "cached-token" });
            
            _httpHandlerStub.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.example.com/",
                CacheToken = true,
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer {{response.token}}" }
                },
                Request = new AdvancedAuthRequest
                {
                    Url = "/login",
                    Method = "POST",
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    },
                    Body = new { username = "admin", password = "password" }
                }
            };

            // First call - should make HTTP request
            var firstResult = await authHandler.ConfigureAuthAsync(_httpClient, config);
            var firstCallCount = _httpHandlerStub.CallCount;

            // Second call - should use cached token
            var secondResult = await authHandler.ConfigureAuthAsync(_httpClient, config);
            var secondCallCount = _httpHandlerStub.CallCount;

            // Assert
            Assert.True(firstResult);
            Assert.True(secondResult);
            Assert.Equal(1, firstCallCount); // First call makes 1 HTTP request
            Assert.Equal(1, secondCallCount); // Second call should be same as first (using cached token, no additional HTTP request)
        }
    }

    internal class HttpMessageHandlerStub : HttpMessageHandler
    {
        public HttpResponseMessage? Response { get; set; }
        public HttpRequestMessage? Request { get; private set; }
        public int CallCount { get; private set; } = 0;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            
            if (Response != null)
            {
                // Return the original response, ensuring content is available and content type is preserved
                var originalContent = await Response.Content.ReadAsStringAsync();
                var clonedResponse = new HttpResponseMessage(Response.StatusCode)
                {
                    Content = new StringContent(originalContent, Encoding.UTF8, "application/json")
                };
                
                // Copy headers if needed
                foreach (var header in Response.Headers)
                {
                    clonedResponse.Headers.Add(header.Key, header.Value);
                }
                
                return clonedResponse;
            }
            
            // Fallback response with proper JSON content type
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        }
        
        public Uri? RequestUri => Request?.RequestUri;
    }
}