using System.Text;
using System.Text.Json;
using ConfigLink.Api;

namespace ConfigLink.Api.Tests.AdvancedAuth
{
    public class AuthenticationTokenTests : IDisposable
    {
        private HttpMessageHandlerStub _httpHandlerStub;
        private HttpClient _httpClient;

        public AuthenticationTokenTests()
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
    }
}