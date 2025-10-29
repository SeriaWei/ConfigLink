using System;
using System.Collections.Generic;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests
{
    public class ConverterTests
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

        #region Converter Tests

        [Fact]
        public void CaseConverter_ShouldConvertToUpperCase()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"hello world\"");
            var rule = CreateRule("case", new { @case = "upper" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("HELLO WORLD", result);
        }

        [Fact]
        public void CaseConverter_ShouldConvertToCamelCase()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"hello world test\"");
            var rule = CreateRule("case", new { @case = "camel" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("helloWorldTest", result);
        }

        [Fact]
        public void CaseConverter_ShouldConvertToPascalCase()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"hello world test\"");
            var rule = CreateRule("case", new { @case = "pascal" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("HelloWorldTest", result);
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
        public void ReplaceConverter_ShouldReplaceText()
        {
            var converter = new ReplaceConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"hello world\"");
            var rule = CreateRule("replace", new { from = "world", to = "universe" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello universe", result);
        }

        [Fact]
        public void SubstringConverter_ShouldExtractSubstring()
        {
            var converter = new SubstringConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"hello world\"");
            var rule = CreateRule("substring", new { start = 0, length = 5 });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello", result);
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

        [Fact]
        public void NumberConverter_ShouldConvertStringToInt()
        {
            var converter = new NumberConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"123\"");
            var rule = CreateRule("number", new { type = "int" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal(123, result);
        }

        [Fact]
        public void BooleanConverter_ShouldConvertStringToBoolean()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"yes\"");
            var rule = CreateRule("boolean", new { output = "boolean" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal(true, result);
        }

        [Fact]
        public void BooleanConverter_ShouldConvertToYesNoFormat()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("true");
            var rule = CreateRule("boolean", new { output = "yesno" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("yes", result);
        }

        #endregion

        #region Simplified Parameter Format Tests

        [Fact]
        public void CaseConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"hello world\"");
            var rule = CreateSimplifiedRule("case", "upper");

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("HELLO WORLD", result);
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

        [Fact]
        public void NumberConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new NumberConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("\"123\"");
            var rule = CreateSimplifiedRule("number", "int");

            var result = converter.Convert(value, rule, null!);

            Assert.Equal(123, result);
        }

        [Fact]
        public void BooleanConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.Deserialize<JsonElement>("true");
            var rule = CreateSimplifiedRule("boolean", "yesno");

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("yes", result);
        }

        #endregion

        // Helper method for simplified parameter format
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
    }
}