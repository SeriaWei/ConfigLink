using ConfigLink;
using ConfigLink.Api;
using System.Text.Json;

namespace Test.Api
{
    /// <summary>
    /// 场景API使用示例
    /// </summary>
    public class ScenarioApiUsageExample
    {
        public static async Task RunExamples()
        {
            // 1. 使用ApiManager直接发送场景数据
            await ApiManagerScenarioExample();

            // 2. 使用MappingEngineWithApi发送场景数据
            await MappingEngineScenarioExample();
        }

        /// <summary>
        /// ApiManager场景示例
        /// </summary>
        private static async Task ApiManagerScenarioExample()
        {
            Console.WriteLine("=== ApiManager Scenario Example ===");

            // 读取配置
            var apiConfigJson = await File.ReadAllTextAsync("config/api.config.json");
            var scenarioConfigJson = await File.ReadAllTextAsync("config/scenario.json");

            // 创建ApiManager，支持场景配置
            var apiConfigs = ApiConfigs.LoadFromJson(apiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(scenarioConfigJson);
            using var apiManager = new ApiManager(apiConfigs, scenarioConfigs);

            // 准备测试数据
            var userData = new
            {
                email = "user@example.com",
                firstName = "John",
                lastName = "Doe"
            };

            // 获取可用场景
            var scenarios = apiManager.GetAvailableScenarios();
            Console.WriteLine($"Available scenarios: {string.Join(", ", scenarios)}");

            // 发送到"subscribe"场景的所有平台
            var results = await apiManager.SendAsync("subscribe", userData);

            Console.WriteLine("Results:");
            foreach (var result in results)
            {
                Console.WriteLine($"Platform: {result.Key}");
                Console.WriteLine($"  Success: {result.Value.Success}");
                Console.WriteLine($"  Status: {result.Value.StatusCode}");
                Console.WriteLine($"  Error: {result.Value.ErrorMessage}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// ApiManager with MappingEngine场景示例
        /// </summary>
        private static async Task MappingEngineScenarioExample()
        {
            Console.WriteLine("=== ApiManager with MappingEngine Scenario Example ===");

            // 读取配置
            var mappingJson = await File.ReadAllTextAsync("config/mapping.json");
            var apiConfigJson = await File.ReadAllTextAsync("config/api.config.json");
            var scenarioConfigJson = await File.ReadAllTextAsync("config/scenario.json");

            // 创建映射引擎和ApiManager，支持映射和场景配置
            var mappingEngine = new MappingEngine(mappingJson);
            var apiConfigs = ApiConfigs.LoadFromJson(apiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(scenarioConfigJson);
            using var apiManager = new ApiManager(apiConfigs, scenarioConfigs);

            // 准备测试数据
            var sourceData = new
            {
                email = "user@example.com",
                firstName = "John",
                lastName = "Doe"
            };

            // 获取可用场景
            var scenarios = apiManager.GetAvailableScenarios();
            Console.WriteLine($"Available scenarios: {string.Join(", ", scenarios)}");

            // 方式1：发送对象数据到场景
            var results1 = await apiManager.SendAsync("subscribe", sourceData);
            Console.WriteLine("Object data results:");
            foreach (var result in results1)
            {
                Console.WriteLine($"Platform: {result.Key} - Success: {result.Value.Success}");
            }

            // 方式2：转换JSON数据并发送到场景
            var sourceJson = JsonSerializer.Serialize(sourceData);
            var (mappedData, results2) = await apiManager.TransformAndSendAsync("subscribe", sourceJson);
            
            Console.WriteLine("Transformed JSON data results:");
            Console.WriteLine($"Mapped data: {JsonSerializer.Serialize(mappedData)}");
            foreach (var result in results2)
            {
                Console.WriteLine($"Platform: {result.Key} - Success: {result.Value.Success}");
            }
        }

        /// <summary>
        /// 演示具体平台配置的映射规则
        /// </summary>
        private static async Task DemonstrateMapping()
        {
            var apiConfigJson = await File.ReadAllTextAsync("config/api.config.json");
            var scenarioConfigJson = await File.ReadAllTextAsync("config/scenario.json");

            var apiConfigs = ApiConfigs.LoadFromJson(apiConfigJson);
            var scenarioConfigs = ScenarioConfigs.LoadFromJson(scenarioConfigJson);
            using var apiManager = new ApiManager(apiConfigs, scenarioConfigs);

            var sourceData = new
            {
                email = "user@example.com",
                firstName = "John",
                lastName = "Doe"
            };

            // 查看每个平台的可用平台
            var platformsInSubscribe = apiManager.GetAvailablePlatforms("subscribe");
            Console.WriteLine($"Platforms in 'subscribe' scenario: {string.Join(", ", platformsInSubscribe)}");

            // 根据scenario.json:
            // PlatformA: email -> emailAddress, firstName -> firstName, lastName -> lastName
            // PlatformB: email -> email, firstName -> fname, lastName -> lname
            
            Console.WriteLine("Expected mapping results:");
            Console.WriteLine("PlatformA will receive: { emailAddress: 'user@example.com', firstName: 'John', lastName: 'Doe' }");
            Console.WriteLine("PlatformB will receive: { email: 'user@example.com', fname: 'John', lname: 'Doe' }");
        }
    }
}