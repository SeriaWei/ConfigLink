using System.Text;
using System.Text.Json;
using ConfigLink.Api;

namespace Test.Api.AdvancedAuth
{
    public class TokenCachingTests : IDisposable
    {
        private HttpMessageHandlerStub _httpHandlerStub;
        private HttpClient _httpClient;

        public TokenCachingTests()
        {
            _httpHandlerStub = new HttpMessageHandlerStub();
            _httpClient = new HttpClient(_httpHandlerStub);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
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
}