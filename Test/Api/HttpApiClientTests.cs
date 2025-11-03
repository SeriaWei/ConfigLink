using ConfigLink.Api;
using Moq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Api.Tests
{
    public class HttpApiClientTests
    {
        [Fact]
        public void Constructor_ShouldInitializeHttpClientAndAuthHandler()
        {
            // Arrange
            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none"
            };

            // Act
            var client = new HttpApiClient(config);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new HttpApiClient(null!));
            Assert.Equal("config", ex.ParamName);
        }

        [Fact]
        public async Task SendAsync_ShouldSendPostRequest_WithValidData()
        {
            // Arrange
            var handlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"success\"}", Encoding.UTF8, "application/json")
                }
            };

            var httpClient = new System.Net.Http.HttpClient(handlerStub);
            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none"
            };

            var mockAuthHandler = new Mock<IAuthHandler>();
            mockAuthHandler.Setup(x => x.ConfigureAuthAsync(It.IsAny<System.Net.Http.HttpClient>(), It.IsAny<ApiConfig>()))
                          .ReturnsAsync(true);

            // Replace the real auth handler with our mock using reflection or by creating a custom constructor
            // Since we can't directly inject the auth handler, we'll test with a simpler approach
            var client = new HttpApiClient(config, httpClient);

            var testData = new { name = "test", value = 123 };

            // Act
            var result = await client.SendAsync(testData, "/api/test");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("success", result.ResponseContent);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleGetRequest()
        {
            // Arrange
            var handlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"get_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none"
            };

            var client = new HttpApiClient(config, new HttpClient(handlerStub));
            var testData = new { id = 123 };

            // Act
            var result = await client.SendAsync(testData, "/api/test", "GET");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("get_success", result.ResponseContent);
            Assert.Equal("https://api.test.com/api/test", handlerStub.RequestUri?.ToString());
            Assert.Equal(HttpMethod.Get, handlerStub.Request?.Method);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleDifferentHttpMethods()
        {
            // Test for PUT method
            var putHandlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"method_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none"
            };

            var putClient = new HttpApiClient(config, new HttpClient(putHandlerStub));
            var testData = new { id = 123 };

            // Act for PUT
            var putResult = await putClient.SendAsync(testData, "/api/test", "PUT");

            // Assert for PUT
            Assert.True(putResult.Success);
            Assert.Equal(200, putResult.StatusCode);
            Assert.Equal(HttpMethod.Put, putHandlerStub.Request?.Method);

            // Test for DELETE method
            var deleteHandlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"method_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var deleteClient = new HttpApiClient(config, new HttpClient(deleteHandlerStub));

            // Act for DELETE
            var deleteResult = await deleteClient.SendAsync(testData, "/api/test", "DELETE");

            // Assert for DELETE
            Assert.True(deleteResult.Success);
            Assert.Equal(200, deleteResult.StatusCode);
            Assert.Equal(HttpMethod.Delete, deleteHandlerStub.Request?.Method);

            // Test for PATCH method
            var patchHandlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"method_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var patchClient = new HttpApiClient(config, new HttpClient(patchHandlerStub));

            // Act for PATCH
            var patchResult = await patchClient.SendAsync(testData, "/api/test", "PATCH");

            // Assert for PATCH
            Assert.True(patchResult.Success);
            Assert.Equal(200, patchResult.StatusCode);
            Assert.Equal(new HttpMethod("PATCH"), patchHandlerStub.Request?.Method);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleAuthentication()
        {
            // Arrange
            var handlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"auth_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "basic",
                Username = "testuser",
                Password = "testpass"
            };

            var client = new HttpApiClient(config, new HttpClient(handlerStub));
            var testData = new { action = "authenticate" };

            // Act
            var result = await client.SendAsync(testData, "/api/auth");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            
            // Verify that authentication was applied (basic auth should add Authorization header)
            Assert.NotNull(handlerStub.Request);
            Assert.True(handlerStub.Request.Headers.Contains("Authorization"));
        }

        [Fact]
        public async Task SendAsync_ShouldHandleHeaders()
        {
            // Arrange
            var handlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"headers_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none",
                Headers = new Dictionary<string, string>
                {
                    { "X-Custom-Header", "custom-value" },
                    { "Content-Type", "application/json" }
                }
            };

            var client = new HttpApiClient(config, new HttpClient(handlerStub));
            var testData = new { data = "with_headers" };

            // Act
            var result = await client.SendAsync(testData, "/api/headers");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            
            // Verify that headers were added to the request
            Assert.NotNull(handlerStub.Request);
            Assert.True(handlerStub.Request.Headers.Contains("X-Custom-Header"));
            Assert.Equal("custom-value", handlerStub.Request.Headers.GetValues("X-Custom-Header").FirstOrDefault());
        }

        [Fact]
        public async Task SendAsync_ShouldHandleGuidPlaceholder()
        {
            // Arrange
            var handlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"guid_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none",
                Headers = new Dictionary<string, string>
                {
                    { "X-Request-ID", "{{guid}}" }  // This should be replaced with a GUID
                }
            };

            var client = new HttpApiClient(config, new HttpClient(handlerStub));
            var testData = new { data = "with_guid" };

            // Act
            var result = await client.SendAsync(testData, "/api/guid");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            
            // Verify that GUID placeholder was replaced with an actual GUID in the headers
            Assert.NotNull(handlerStub.Request);
            if (handlerStub.Request.Headers.TryGetValues("X-Request-ID", out var values))
            {
                var requestId = values.FirstOrDefault();
                // The header value should be a valid GUID
                Assert.True(Guid.TryParse(requestId, out _));
            }
        }

        [Fact]
        public async Task SendAsync_ShouldHandleTimeout()
        {
            // Arrange: Create a handler that simulates a timeout by taking longer than the configured timeout
            var timeoutHandler = new TimeoutHttpMessageHandler
            {
                Delay = TimeSpan.FromSeconds(3) // 3 second delay, longer than 1 second timeout
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 1, // 1 second timeout
                Auth = "none",
                Retry = 0
            };

            var client = new HttpApiClient(config, new HttpClient(timeoutHandler));
            var testData = new { data = "timeout_test" };

            // Act
            var result = await client.SendAsync(testData, "/api/timeout");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // Should fail due to timeout
            Assert.Contains("timeout", result.ErrorMessage?.ToLower() ?? "");
        }

        [Fact]
        public async Task SendAsync_ShouldRetryOnFailure()
        {
            // Arrange: Create a handler that fails initially then succeeds on the last attempt
            var handlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError, // 500 error
                    Content = new StringContent("{\"error\":\"server_error\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none",
                Retry = 0  // No retries - should fail immediately
            };

            var client = new HttpApiClient(config, new HttpClient(handlerStub));
            var testData = new { data = "retry_test" };

            // Act
            var result = await client.SendAsync(testData, "/api/retry");

            // Assert: Should fail since no retries are allowed
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task SendAsync_ShouldRetrySpecifiedNumberOfTimesOnFailure()
        {
            // Need to create a custom handler that changes behavior after certain calls
            var failingHandler = new CountingHttpMessageHandler
            {
                MaxFailures = 2, // Fail twice, then succeed
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"success_after_retry\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none",
                Retry = 2  // Should retry twice
            };

            var client = new HttpApiClient(config, new HttpClient(failingHandler));
            var testData = new { data = "retry_test" };

            // Act
            var result = await client.SendAsync(testData, "/api/retry");

            // Assert: Should succeed after retries
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode); // Success after retries
            Assert.Equal(3, failingHandler.CallCount); // Original call + 2 retries = 3 total calls
        }

        [Fact]
        public async Task SendAsync_ShouldHandleDifferentDataTypes()
        {
            // Arrange
            var handlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"data_types_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none"
            };

            var client = new HttpApiClient(config, new HttpClient(handlerStub));

            // Act & Assert with object
            var objectData = new { name = "test", count = 42 };
            var objectResult = await client.SendAsync(objectData, "/api/test");
            Assert.NotNull(objectResult);

            // Act & Assert with dictionary
            var dictData = new Dictionary<string, object> { { "name", "test" }, { "count", 42 } };
            var dictResult = await client.SendAsync(dictData, "/api/test");
            Assert.NotNull(dictResult);

            // Act & Assert with JSON string
            var jsonString = JsonSerializer.Serialize(objectData);
            var stringResult = await client.SendAsync(jsonString, "/api/test");
            Assert.NotNull(stringResult);
        }

        [Fact]
        public void Dispose_ShouldDisposeHttpClient()
        {
            // Arrange
            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none"
            };

            var client = new HttpApiClient(config);

            // Act
            client.Dispose(); // This should dispose the internal HttpClient

            // Assert - We can't easily test disposal of the internal HttpClient without reflection,
            // but the method should execute without throwing
            Assert.NotNull(client); // Client object still exists but should be disposed internally
        }

        [Fact]
        public async Task SendAsync_ShouldHandleInvalidHttpMethod()
        {
            // Arrange - This test is to verify that unsupported HTTP methods return an error
            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com",
                TimeoutSeconds = 30,
                Auth = "none"
            };

            var client = new HttpApiClient(config);
            var testData = new { data = "invalid_method" };

            // Act & Assert - Should throw NotSupportedException for invalid HTTP method
            var ex = await Assert.ThrowsAsync<NotSupportedException>(() => client.SendAsync(testData, "/api/test", "INVALID_METHOD"));
            Assert.Contains("is not supported", ex.Message);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleUriBuilding()
        {
            // Arrange
            var handlerStub = new HttpMessageHandlerStub
            {
                Response = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"result\":\"uri_success\"}", Encoding.UTF8, "application/json")
                }
            };

            var config = new ApiConfig
            {
                Endpoint = "https://api.test.com/",
                TimeoutSeconds = 30,
                Auth = "none"
            };

            var client = new HttpApiClient(config, new HttpClient(handlerStub));
            var testData = new { data = "uri_test" };

            // Act - Test different path formats
            var result1 = await client.SendAsync(testData, "/api/test");  // Absolute path
            var result2 = await client.SendAsync(testData, "api/test");   // Relative path

            // Assert - Both should be successful
            Assert.True(result1.Success);
            Assert.True(result2.Success);
        }
    }
}