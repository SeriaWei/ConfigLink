using ConfigLink.Api;
using System.Text.Json;

namespace ConfigLink
{
    /// <summary>
    /// API管理器，负责管理API配置和执行API调用
    /// </summary>
    public class ApiManager : IDisposable
    {
        private readonly ApiConfigs _apiConfigs;
        private readonly ScenarioConfigs _scenarioConfigs;
        private readonly IHttpApiClientFactory _clientFactory;
        private readonly Dictionary<string, IHttpApiClient> _clients;
        private bool _disposed = false;

        public ApiManager(ApiConfigs apiConfigs, ScenarioConfigs scenarioConfigs) 
            : this(apiConfigs, scenarioConfigs, new HttpApiClientFactory())
        {
        }

        public ApiManager(ApiConfigs apiConfigs, ScenarioConfigs scenarioConfigs, IHttpApiClientFactory clientFactory)
        {
            _apiConfigs = apiConfigs ?? throw new ArgumentNullException(nameof(apiConfigs));
            _scenarioConfigs = scenarioConfigs ?? throw new ArgumentNullException(nameof(scenarioConfigs));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _clients = new Dictionary<string, IHttpApiClient>();
        }

        /// <summary>
        /// 获取所有可用的场景名称
        /// </summary>
        public IEnumerable<string> GetAvailableScenarios()
        {
            return _scenarioConfigs.GetAvailableScenarios();
        }

        /// <summary>
        /// 获取指定场景中的所有平台名称
        /// </summary>
        /// <param name="scenarioName">场景名称</param>
        public IEnumerable<string> GetAvailablePlatforms(string scenarioName)
        {
            return _scenarioConfigs.GetAvailablePlatforms(scenarioName);
        }

        /// <summary>
        /// 基于场景发送数据到所有相关平台
        /// </summary>
        /// <param name="scenarioName">场景名称</param>
        /// <param name="data">要发送的数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>各平台的API调用结果</returns>
        public async Task<Dictionary<string, ApiResult>> SendAsync(string scenarioName, object data, CancellationToken cancellationToken = default)
        {
            var scenario = _scenarioConfigs.GetScenario(scenarioName);
            if (scenario == null)
            {
                var errorResult = new ApiResult
                {
                    Success = false,
                    ErrorMessage = $"Scenario '{scenarioName}' not found in configuration"
                };
                return new Dictionary<string, ApiResult> { { scenarioName, errorResult } };
            }

            var results = new Dictionary<string, ApiResult>();
            var tasks = scenario.Select(async platformConfig => new
            {
                Platform = platformConfig.Key,
                Result = await SendToPlatformInScenarioAsync(scenarioName, platformConfig.Key, data, cancellationToken)
            });

            var platformResults = await Task.WhenAll(tasks);
            foreach (var platformResult in platformResults)
            {
                results[platformResult.Platform] = platformResult.Result;
            }

            return results;
        }

        /// <summary>
        /// 使用场景配置中的映射规则转换数据并发送到所有相关平台
        /// </summary>
        /// <param name="scenarioName">场景名称</param>
        /// <param name="sourceJson">源数据JSON</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>转换结果和各平台的API调用结果</returns>
        public async Task<(Dictionary<string, Dictionary<string, object?>> TransformedData, Dictionary<string, ApiResult> ApiResults)> TransformAndSendAsync(string scenarioName, string sourceJson, CancellationToken cancellationToken = default)
        {
            var scenario = _scenarioConfigs.GetScenario(scenarioName);
            if (scenario == null)
            {
                var errorResult = new ApiResult
                {
                    Success = false,
                    ErrorMessage = $"Scenario '{scenarioName}' not found in configuration"
                };
                return (new Dictionary<string, Dictionary<string, object?>>(), 
                       new Dictionary<string, ApiResult> { { scenarioName, errorResult } });
            }

            // 解析源数据
            Dictionary<string, object?> sourceData;
            try
            {
                sourceData = JsonSerializer.Deserialize<Dictionary<string, object?>>(sourceJson) ?? new Dictionary<string, object?>();
            }
            catch (Exception ex)
            {
                var errorResult = new ApiResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to parse source JSON: {ex.Message}"
                };
                return (new Dictionary<string, Dictionary<string, object?>>(), 
                       new Dictionary<string, ApiResult> { { scenarioName, errorResult } });
            }

            var transformedData = new Dictionary<string, Dictionary<string, object?>>();
            var apiResults = new Dictionary<string, ApiResult>();

            // 并行处理各平台
            var tasks = scenario.Select(async platformConfig =>
            {
                var platformName = platformConfig.Key;
                var platformMappings = platformConfig.Value.Mappings;
                
                // 为每个平台应用特定的映射规则
                var mappedData = ApplyMappings(sourceData, platformMappings);
                transformedData[platformName] = mappedData;
                
                // 发送到平台
                var result = await SendToPlatformInScenarioAsync(scenarioName, platformName, mappedData, cancellationToken);
                return new { Platform = platformName, Result = result };
            });

            var platformResults = await Task.WhenAll(tasks);
            foreach (var platformResult in platformResults)
            {
                apiResults[platformResult.Platform] = platformResult.Result;
            }

            return (transformedData, apiResults);
        }

        /// <summary>
        /// 基于场景发送数据到指定平台
        /// </summary>
        /// <param name="scenarioName">场景名称</param>
        /// <param name="platformName">平台名称</param>
        /// <param name="data">要发送的数据</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>API调用结果</returns>
        public async Task<ApiResult> SendToPlatformInScenarioAsync(string scenarioName, string platformName, object data, CancellationToken cancellationToken = default)
        {
            var platformConfig = _scenarioConfigs.GetPlatformConfig(scenarioName, platformName);
            if (platformConfig == null)
            {
                return new ApiResult
                {
                    Success = false,
                    ErrorMessage = $"Platform '{platformName}' not found in scenario '{scenarioName}'"
                };
            }

            if (!_apiConfigs.TryGetValue(platformName, out var apiConfig))
            {
                return new ApiResult
                {
                    Success = false,
                    ErrorMessage = $"API configuration for platform '{platformName}' not found"
                };
            }

            // 应用字段映射
            var mappedData = ApplyMappings(data, platformConfig.Mappings);

            // 创建或获取HTTP客户端
            var client = GetOrCreateClient(platformName, apiConfig);
            
            // 发送请求到指定路径
            return await client.SendAsync(mappedData, platformConfig.Path, platformConfig.Method, cancellationToken);
        }

        /// <summary>
        /// 应用字段映射规则
        /// </summary>
        /// <param name="sourceData">源数据</param>
        /// <param name="mappings">映射规则</param>
        /// <returns>映射后的数据</returns>
        private Dictionary<string, object?> ApplyMappings(object sourceData, List<MappingRule> mappings)
        {
            var result = new Dictionary<string, object?>();
            
            // 将源数据转换为字典
            Dictionary<string, object?> sourceDict;
            if (sourceData is Dictionary<string, object?> dict)
            {
                sourceDict = dict;
            }
            else if (sourceData is string jsonString)
            {
                try
                {
                    sourceDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonString) ?? new Dictionary<string, object?>();
                }
                catch
                {
                    sourceDict = new Dictionary<string, object?>();
                }
            }
            else
            {
                // 使用JSON序列化和反序列化来转换对象
                try
                {
                    var json = JsonSerializer.Serialize(sourceData);
                    sourceDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? new Dictionary<string, object?>();
                }
                catch
                {
                    sourceDict = new Dictionary<string, object?>();
                }
            }

            // 应用映射规则
            foreach (var mapping in mappings)
            {
                if (sourceDict.TryGetValue(mapping.Source, out var value))
                {
                    result[mapping.Target] = value;
                }
            }

            return result;
        }

        private IHttpApiClient GetOrCreateClient(string platformName, ApiConfig config)
        {
            if (!_clients.TryGetValue(platformName, out var client))
            {
                client = _clientFactory.CreateClient(config);
                _clients[platformName] = client;
            }
            return client;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var client in _clients.Values)
                {
                    client?.Dispose();
                }
                _clients.Clear();
                _disposed = true;
            }
        }
    }
}