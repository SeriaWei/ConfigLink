using System.Text.Json.Serialization;

namespace ConfigLink.Api
{
    /// <summary>
    /// 场景配置类，表示单个平台在特定场景下的配置
    /// </summary>
    public class ScenarioPlatformConfig
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = "POST";

        [JsonPropertyName("mappings")]
        public List<MappingRule> Mappings { get; set; } = new List<MappingRule>();
    }

    /// <summary>
    /// 场景配置集合，包含所有场景的配置
    /// </summary>
    public class ScenarioConfigs : Dictionary<string, Dictionary<string, ScenarioPlatformConfig>>
    {
        public static ScenarioConfigs LoadFromJson(string json)
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<ScenarioConfigs>(json);
            return result ?? new ScenarioConfigs();
        }

        /// <summary>
        /// 获取指定场景的所有平台配置
        /// </summary>
        /// <param name="scenarioName">场景名称</param>
        /// <returns>平台配置字典</returns>
        public Dictionary<string, ScenarioPlatformConfig>? GetScenario(string scenarioName)
        {
            return TryGetValue(scenarioName, out var scenario) ? scenario : null;
        }

        /// <summary>
        /// 获取指定场景中指定平台的配置
        /// </summary>
        /// <param name="scenarioName">场景名称</param>
        /// <param name="platformName">平台名称</param>
        /// <returns>平台配置</returns>
        public ScenarioPlatformConfig? GetPlatformConfig(string scenarioName, string platformName)
        {
            var scenario = GetScenario(scenarioName);
            return scenario?.TryGetValue(platformName, out var platform) == true ? platform : null;
        }

        /// <summary>
        /// 获取所有可用的场景名称
        /// </summary>
        /// <returns>场景名称列表</returns>
        public IEnumerable<string> GetAvailableScenarios()
        {
            return Keys;
        }

        /// <summary>
        /// 获取指定场景中的所有平台名称
        /// </summary>
        /// <param name="scenarioName">场景名称</param>
        /// <returns>平台名称列表</returns>
        public IEnumerable<string> GetAvailablePlatforms(string scenarioName)
        {
            var scenario = GetScenario(scenarioName);
            return scenario?.Keys ?? Enumerable.Empty<string>();
        }
    }
}