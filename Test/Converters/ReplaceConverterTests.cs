using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class ReplaceConverterTests
    {
        private MappingRule CreateRule(string converterType, object? conversionParams = null)
        {
            var rule = new MappingRule
            {
                Conversion = new List<string> { converterType }
            };

            if (conversionParams != null)
            {
                // 直接使用转换器类型和参数创建正确的嵌套结�?
                var innerParamsJson = JsonSerializer.Serialize(conversionParams);
                var outerJson = $@"{{ ""{converterType.ToLowerInvariant()}"": {innerParamsJson} }}";
                
                rule.ConversionParams = JsonSerializer.Deserialize<Dictionary<string, object>>(outerJson);
            }

            return rule;
        }

        [Fact]
        public void ReplaceConverter_ShouldReplaceText()
        {
            var converter = new ReplaceConverter();
            var value = JsonSerializer.SerializeToElement("hello world");
            var rule = CreateRule("replace", new { from = "world", to = "universe" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello universe", result);
        }
    }
}
