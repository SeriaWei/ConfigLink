using ConfigLink;
using ConfigLink.Api;
using Moq;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Api.Tests
{
    public class ApiManagerMockTests
    {
        private const string ApiConfigJson = @"{
            ""TestPlatform"": {
                ""endpoint"": ""https://api.test.com"",
                ""auth"": ""none"",
                ""headers"": {
                    ""Content-Type"": ""application/json""
                }
            },
            ""EchoPlatform"": {
                ""endpoint"": ""https://api.echo.com"",
                ""auth"": ""none"",
                ""headers"": {
                    ""Content-Type"": ""application/json""
                }
            }
        }";

        private const string ScenarioConfigJson = @"{
            ""test"": {
                ""TestPlatform"": {
                    ""path"": ""/api/test"",
                    ""method"": ""POST"",
                    ""mappings"": [
                        {
                            ""source"": ""name"",
                            ""target"": ""userName""
                        },
                        {
                            ""source"": ""email"",
                            ""target"": ""emailAddress""
                        }
                    ]
                },
                ""EchoPlatform"": {
                    ""path"": ""/api/echo"",
                    ""method"": ""POST"",
                    ""mappings"": [
                        {
                            ""source"": ""name"",
                            ""target"": ""fullName""
                        },
                        {
                            ""source"": ""email"",
                            ""target"": ""email""
                        }
                    ]
                }
            }
        }";

        [Fact]
        public async Task ApiManager_ShouldSendToScenario_WithMock()
        {
            var apiConfigs = ApiConfigs.LoadFromJson(ApiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(ScenarioConfigJson);

            var mockFactory = new Mock<IHttpApiClientFactory>();
            var mockTestClient = new Mock<IHttpApiClient>();
            var mockEchoClient = new Mock<IHttpApiClient>();

            var testPlatformResult = new ApiResult
            {
                Success = true,
                StatusCode = 200,
                ResponseContent = @"{""status"": ""success"", ""platform"": ""test""}",
                Duration = TimeSpan.FromMilliseconds(100)
            };

            var echoPlatformResult = new ApiResult
            {
                Success = true,
                StatusCode = 200,
                ResponseContent = @"{""status"": ""success"", ""platform"": ""echo""}",
                Duration = TimeSpan.FromMilliseconds(150)
            };

            mockTestClient
                .Setup(c => c.SendAsync(It.IsAny<object>(), "/api/test", "POST", It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPlatformResult);

            mockEchoClient
                .Setup(c => c.SendAsync(It.IsAny<object>(), "/api/echo", "POST", It.IsAny<CancellationToken>()))
                .ReturnsAsync(echoPlatformResult);

            mockFactory
                .Setup(f => f.CreateClient(It.Is<ApiConfig>(c => c.Endpoint == "https://api.test.com")))
                .Returns(mockTestClient.Object);

            mockFactory
                .Setup(f => f.CreateClient(It.Is<ApiConfig>(c => c.Endpoint == "https://api.echo.com")))
                .Returns(mockEchoClient.Object);

            var apiManager = new ApiManager(apiConfigs, scenarioConfigs, mockFactory.Object);
            var testData = new
            {
                name = "John Doe",
                email = "john@example.com"
            };

            var results = await apiManager.SendAsync("test", testData);

            Assert.Equal(2, results.Count);
            Assert.Contains("TestPlatform", results.Keys);
            Assert.Contains("EchoPlatform", results.Keys);

            var testResult = results["TestPlatform"];
            var echoResult = results["EchoPlatform"];

            Assert.True(testResult.Success);
            Assert.Equal(200, testResult.StatusCode);
            Assert.Contains("test", testResult.ResponseContent);

            Assert.True(echoResult.Success);
            Assert.Equal(200, echoResult.StatusCode);
            Assert.Contains("echo", echoResult.ResponseContent);

            mockTestClient.Verify(c => c.SendAsync(
                It.Is<object>(data => VerifyMappedData(data, "userName", "John Doe")), 
                "/api/test", 
                "POST", 
                It.IsAny<CancellationToken>()), Times.Once);

            mockEchoClient.Verify(c => c.SendAsync(
                It.Is<object>(data => VerifyMappedData(data, "fullName", "John Doe")), 
                "/api/echo", 
                "POST", 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApiManager_ShouldHandleFailure_WithMock()
        {
            var apiConfigs = ApiConfigs.LoadFromJson(ApiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(ScenarioConfigJson);

            var mockFactory = new Mock<IHttpApiClientFactory>();
            var mockTestClient = new Mock<IHttpApiClient>();
            var mockEchoClient = new Mock<IHttpApiClient>();

            var failedResult = new ApiResult
            {
                Success = false,
                StatusCode = 500,
                ErrorMessage = "Internal Server Error",
                Duration = TimeSpan.FromMilliseconds(200)
            };

            var successResult = new ApiResult
            {
                Success = true,
                StatusCode = 200,
                ResponseContent = @"{""status"": ""success""}",
                Duration = TimeSpan.FromMilliseconds(100)
            };

            mockTestClient
                .Setup(c => c.SendAsync(It.IsAny<object>(), "/api/test", "POST", It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);

            mockEchoClient
                .Setup(c => c.SendAsync(It.IsAny<object>(), "/api/echo", "POST", It.IsAny<CancellationToken>()))
                .ReturnsAsync(successResult);

            mockFactory
                .Setup(f => f.CreateClient(It.Is<ApiConfig>(c => c.Endpoint == "https://api.test.com")))
                .Returns(mockTestClient.Object);

            mockFactory
                .Setup(f => f.CreateClient(It.Is<ApiConfig>(c => c.Endpoint == "https://api.echo.com")))
                .Returns(mockEchoClient.Object);

            var apiManager = new ApiManager(apiConfigs, scenarioConfigs, mockFactory.Object);
            var testData = new { name = "Test User", email = "test@example.com" };

            var results = await apiManager.SendAsync("test", testData);

            Assert.Equal(2, results.Count);
            
            var testResult = results["TestPlatform"];
            Assert.False(testResult.Success);
            Assert.Equal(500, testResult.StatusCode);
            Assert.Equal("Internal Server Error", testResult.ErrorMessage);

            var echoResult = results["EchoPlatform"];
            Assert.True(echoResult.Success);
            Assert.Equal(200, echoResult.StatusCode);
        }

        [Fact]
        public async Task ApiManager_ShouldApplyDataMappings_WithMock()
        {
            var apiConfigs = ApiConfigs.LoadFromJson(ApiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(ScenarioConfigJson);

            var mockFactory = new Mock<IHttpApiClientFactory>();
            var mockClient = new Mock<IHttpApiClient>();

            var result = new ApiResult
            {
                Success = true,
                StatusCode = 200,
                ResponseContent = "{}",
                Duration = TimeSpan.FromMilliseconds(50)
            };

            mockClient
                .Setup(c => c.SendAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            mockFactory
                .Setup(f => f.CreateClient(It.IsAny<ApiConfig>()))
                .Returns(mockClient.Object);

            var apiManager = new ApiManager(apiConfigs, scenarioConfigs, mockFactory.Object);
            var testData = new
            {
                name = "Jane Smith",
                email = "jane.smith@example.com"
            };

            await apiManager.SendAsync("test", testData);

            mockClient.Verify(c => c.SendAsync(
                It.Is<object>(data => 
                    VerifyMappedData(data, "userName", "Jane Smith") || 
                    VerifyMappedData(data, "fullName", "Jane Smith")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.AtLeast(1));

            mockClient.Verify(c => c.SendAsync(
                It.Is<object>(data => 
                    VerifyMappedData(data, "emailAddress", "jane.smith@example.com") || 
                    VerifyMappedData(data, "email", "jane.smith@example.com")),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.AtLeast(1));
        }

        [Fact]
        public void ApiManager_ShouldThrowForInvalidScenario_WithMock()
        {
            var apiConfigs = ApiConfigs.LoadFromJson(ApiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(ScenarioConfigJson);
            var mockFactory = new Mock<IHttpApiClientFactory>();

            var apiManager = new ApiManager(apiConfigs, scenarioConfigs, mockFactory.Object);

            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await apiManager.SendAsync("nonexistent", new { }));
        }

        private bool VerifyMappedData(object data, string expectedField, string expectedValue)
        {
            if (data is Dictionary<string, object> dict)
            {
                return dict.ContainsKey(expectedField) && 
                       dict[expectedField]?.ToString() == expectedValue;
            }

            var prop = data.GetType().GetProperty(expectedField);
            if (prop != null)
            {
                var value = prop.GetValue(data);
                return value?.ToString() == expectedValue;
            }

            return false;
        }
    }
}