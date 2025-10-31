using ConfigLink;
using ConfigLink.Api;
using Xunit;
using System.Text.Json;
using System.Collections.Generic;

namespace Test.Api
{
    public class ScenarioApiTests
    {
        private const string ApiConfigJson = @"{
            ""PlatformA"": {
                ""endpoint"": ""https://api.a.com"",
                ""auth"": ""none""
            },
            ""PlatformB"": {
                ""endpoint"": ""https://api.b.com"",
                ""auth"": ""none""
            }
        }";

        private const string ScenarioConfigJson = @"{
            ""subscribe"": {
                ""PlatformA"": {
                    ""path"": ""/api/v1/subscribe"",
                    ""method"": ""POST"",
                    ""mappings"": [
                        {
                            ""source"": ""email"",
                            ""target"": ""emailAddress""
                        },
                        {
                            ""source"": ""firstName"",
                            ""target"": ""firstName""
                        },
                        {
                            ""source"": ""lastName"",
                            ""target"": ""lastName""
                        }
                    ]
                },
                ""PlatformB"": {
                    ""path"": ""/v2/newsletter/subscribe"",
                    ""method"": ""PUT"",
                    ""mappings"": [
                        {
                            ""source"": ""email"",
                            ""target"": ""email""
                        },
                        {
                            ""source"": ""firstName"",
                            ""target"": ""fname""
                        },
                        {
                            ""source"": ""lastName"",
                            ""target"": ""lname""
                        }
                    ]
                }
            }
        }";

        [Fact]
        public void TestScenarioConfigLoading()
        {
            // 测试场景配置加载
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(ScenarioConfigJson);
            
            Assert.NotNull(scenarioConfigs);
            Assert.True(scenarioConfigs.ContainsKey("subscribe"));
            
            var subscribeScenario = scenarioConfigs["subscribe"];
            Assert.Equal(2, subscribeScenario.Count);
            Assert.True(subscribeScenario.ContainsKey("PlatformA"));
            Assert.True(subscribeScenario.ContainsKey("PlatformB"));

            // 测试PlatformA配置
            var platformA = subscribeScenario["PlatformA"];
            Assert.Equal("/api/v1/subscribe", platformA.Path);
            Assert.Equal("POST", platformA.Method);
            Assert.Equal(3, platformA.Mappings.Count);

            // 测试PlatformB配置
            var platformB = subscribeScenario["PlatformB"];
            Assert.Equal("/v2/newsletter/subscribe", platformB.Path);
            Assert.Equal("PUT", platformB.Method);
            Assert.Equal(3, platformB.Mappings.Count);
        }

        [Fact]
        public void TestApiManagerWithScenario()
        {
            // 测试ApiManager支持场景配置
            var apiConfigs = ApiConfigs.LoadFromJson(ApiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(ScenarioConfigJson);
            var apiManager = new ApiManager(apiConfigs, scenarioConfigs);
            
            // 测试获取场景信息
            var scenarios = apiManager.GetAvailableScenarios().ToList();
            Assert.Single(scenarios);
            Assert.Equal("subscribe", scenarios[0]);

            // 测试获取场景中的平台
            var platforms = apiManager.GetAvailablePlatforms("subscribe").ToList();
            Assert.Equal(2, platforms.Count);
            Assert.Contains("PlatformA", platforms);
            Assert.Contains("PlatformB", platforms);
        }

        [Fact]
        public void TestApiManagerWithMappingEngine()
        {
            var apiConfigs = ApiConfigs.LoadFromJson(ApiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(ScenarioConfigJson);
            var apiManager = new ApiManager(apiConfigs, scenarioConfigs);
            
            // 测试获取场景信息
            var scenarios = apiManager.GetAvailableScenarios().ToList();
            Assert.Single(scenarios);
            Assert.Equal("subscribe", scenarios[0]);

            // 测试获取场景中的平台
            var platforms = apiManager.GetAvailablePlatforms("subscribe").ToList();
            Assert.Equal(2, platforms.Count);
            Assert.Contains("PlatformA", platforms);
            Assert.Contains("PlatformB", platforms);
        }

        [Fact]
        public void TestDataMappingForScenario()
        {
            // 测试数据映射功能
            var apiConfigs = ApiConfigs.LoadFromJson(ApiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(ScenarioConfigJson);
            var apiManager = new ApiManager(apiConfigs, scenarioConfigs);
            
            var sourceData = new
            {
                email = "test@example.com",
                firstName = "John",
                lastName = "Doe"
            };

            // 这里我们无法直接测试ApplyMappings方法，因为它是私有的
            // 但我们可以通过检查配置来验证映射规则是否正确加载
            var platformAConfig = scenarioConfigs.GetPlatformConfig("subscribe", "PlatformA");
            var platformBConfig = scenarioConfigs.GetPlatformConfig("subscribe", "PlatformB");

            Assert.NotNull(platformAConfig);
            Assert.NotNull(platformBConfig);

            // 验证PlatformA的映射规�?
            var platformAMappings = platformAConfig.Mappings;
            Assert.Equal("email", platformAMappings[0].Source);
            Assert.Equal("emailAddress", platformAMappings[0].Target);
            Assert.Equal("firstName", platformAMappings[1].Source);
            Assert.Equal("firstName", platformAMappings[1].Target);
            Assert.Equal("lastName", platformAMappings[2].Source);
            Assert.Equal("lastName", platformAMappings[2].Target);

            // 验证PlatformB的映射规�?
            var platformBMappings = platformBConfig.Mappings;
            Assert.Equal("email", platformBMappings[0].Source);
            Assert.Equal("email", platformBMappings[0].Target);
            Assert.Equal("firstName", platformBMappings[1].Source);
            Assert.Equal("fname", platformBMappings[1].Target);
            Assert.Equal("lastName", platformBMappings[2].Source);
            Assert.Equal("lname", platformBMappings[2].Target);
        }
    }
}
