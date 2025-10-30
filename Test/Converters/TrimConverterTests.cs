using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class TrimConverterTests
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

        private MappingRule CreateSimplifiedRule(string converterName, string parameterValue)
        {
            var rule = new MappingRule
            {
                Target = "test",
                Source = "test",
                Conversion = new List<string> { converterName }
            };

            // Create simplified parameter format: {"converterName": "value"}
            var conversionParams = new Dictionary<string, object?>
            {
                [converterName] = parameterValue
            };

            var jsonString = JsonSerializer.Serialize(conversionParams);
            var jsonDoc = JsonDocument.Parse(jsonString);
            rule.ConversionParams = jsonDoc.RootElement.EnumerateObject()
                                         .ToDictionary(p => p.Name, p => (object)p.Value);

            return rule;
        }

        [Fact]
        public void TrimConverter_ShouldTrimWhitespace()
        {
            var converter = new TrimConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"  hello world  \"");
            var rule = CreateRule("trim", new { type = "both" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello world", result);
        }

        [Fact]
        public void TrimConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new TrimConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"  hello world  \"");
            var rule = CreateSimplifiedRule("trim", "both");

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello world", result);
        }
    }
}