using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class DefaultConverterTests
    {
        private MappingRule CreateRule(string converterType, object? conversionParams = null)
        {
            var rule = new MappingRule
            {
                Conversion = new List<string> { converterType }
            };

            if (conversionParams != null)
            {
                // 直接使用转换器类型和参数创建正确的嵌套结构
                var innerParamsJson = JsonSerializer.Serialize(conversionParams);
                var outerJson = $@"{{ ""{converterType.ToLowerInvariant()}"": {innerParamsJson} }}";
                
                rule.ConversionParams = JsonSerializer.Deserialize<Dictionary<string, object>>(outerJson);
            }

            return rule;
        }

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForNull()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("null");
            var rule = CreateRule("default", new { value = "default text", condition = "null" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("default text", result);
        }
    }
}