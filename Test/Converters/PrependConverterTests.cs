using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class PrependConverterTests
    {
        private MappingRule CreateRule(string converterType, object? conversionParams = null)
        {
            var rule = new MappingRule
            {
                Conversion = new List<string> { converterType }
            };

            if (conversionParams != null)
            {
                rule.ConversionParams = new Dictionary<string, object>();
                
                // 使用反射获取参数对象的属性
                var properties = conversionParams.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(conversionParams);
                    if (value != null)
                    {
                        rule.ConversionParams[prop.Name] = value;
                    }
                }
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
        public void PrependConverter_ShouldPrependToString()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"world\"");
            var rule = CreateRule("prepend", new { prepend = "Hello " });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("Hello world", result);
        }

        [Fact]
        public void PrependConverter_ShouldPrependToNumber()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("123");
            var rule = CreateRule("prepend", new { prepend = "Number: " });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("Number: 123", result);
        }

        [Fact]
        public void PrependConverter_ShouldHandleNullValue()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("null");
            var rule = CreateRule("prepend", new { prepend = "Prefix" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("Prefix", result);
        }

        [Fact]
        public void PrependConverter_ShouldPrependToArray()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("[1, 2, 3]");
            var rule = CreateRule("prepend", new { prepend = "Array: " });

            var result = converter.Convert(value, rule, null!);

            // The actual result includes spaces in JSON format
            Assert.Equal("Array: [1, 2, 3]", result);
        }

        [Fact]
        public void PrependConverter_ShouldHandleEmptyPrefix()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"test\"");
            var rule = CreateRule("prepend", new { prepend = "" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("test", result);
        }

        [Fact]
        public void PrependConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"text\"");
            var rule = CreateSimplifiedRule("prepend", "PREFIX: ");

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("PREFIX: text", result);
        }
    }
}