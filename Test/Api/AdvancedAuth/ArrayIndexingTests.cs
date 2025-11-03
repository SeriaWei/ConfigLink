using System.Text;
using System.Text.Json;
using ConfigLink.Api;

namespace ConfigLink.Api.Tests.AdvancedAuth
{
    public class ArrayIndexingTests : IDisposable
    {
        private HttpMessageHandlerStub _httpHandlerStub;
        private HttpClient _httpClient;

        public ArrayIndexingTests()
        {
            _httpHandlerStub = new HttpMessageHandlerStub();
            _httpClient = new HttpClient(_httpHandlerStub);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        [Fact]
        public async Task ConfigureAuthAsync_ArrayIndexing_ExtractsValueCorrectly()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();
            var jsonResponse = JsonSerializer.Serialize(new { tokens = new[] { "first-token", "second-token", "third-token" } });
            
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
                    { "Authorization", "Bearer {{response.tokens[0]}}" } // Access first element
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
            Assert.Equal("Bearer first-token", authHeader);
        }

        [Fact]
        public async Task ConfigureAuthAsync_ArrayIndexing_MultiplePositions_ExtractsCorrectly()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();
            var jsonResponse = JsonSerializer.Serialize(new { 
                tokens = new[] { "first-token", "second-token", "third-token" },
                numbers = new[] { 100, 200, 300 }
            });
            
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
                    { "X-First-Token", "{{response.tokens[0]}}" },
                    { "X-Second-Token", "{{response.tokens[1]}}" },
                    { "X-Third-Token", "{{response.tokens[2]}}" },
                    { "X-Number", "{{response.numbers[1]}}" } // Access second number (200)
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
            Assert.Equal("first-token", _httpClient.DefaultRequestHeaders.GetValues("X-First-Token").First());
            Assert.Equal("second-token", _httpClient.DefaultRequestHeaders.GetValues("X-Second-Token").First());
            Assert.Equal("third-token", _httpClient.DefaultRequestHeaders.GetValues("X-Third-Token").First());
            Assert.Equal("200", _httpClient.DefaultRequestHeaders.GetValues("X-Number").First());
        }
        
        [Fact]
        public async Task ConfigureAuthAsync_ArrayIndexingWithNestedProperty_ExtractsCorrectly()
        {
            // Arrange
            var authHandler = new AdvancedAuthHandler();
            var jsonResponse = JsonSerializer.Serialize(new { 
                items = new[] { 
                    new { id = 1, value = "first-value" }, 
                    new { id = 2, value = "second-value" },
                    new { id = 3, value = "third-value" }
                }
            });
            
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
                    { "X-Item-Value", "{{response.items[1].value}}" } // Access second item's value
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
            Assert.Equal("second-value", _httpClient.DefaultRequestHeaders.GetValues("X-Item-Value").First());
        }
    }
}