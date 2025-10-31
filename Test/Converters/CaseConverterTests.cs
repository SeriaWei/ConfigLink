using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class CaseConverterTests
    {

        [Fact]
        public void CaseConverter_ShouldConvertToUpperCase()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.SerializeToElement("hello world");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "case" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["case"] = new { @case = "upper" }
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("HELLO WORLD", result);
        }

        [Fact]
        public void CaseConverter_ShouldConvertToCamelCase()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.SerializeToElement("hello world test");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "case" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["case"] = new { @case = "camel" }
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("helloWorldTest", result);
        }

        [Fact]
        public void CaseConverter_ShouldConvertToPascalCase()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.SerializeToElement("hello world test");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "case" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["case"] = new { @case = "pascal" }
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("HelloWorldTest", result);
        }

        [Fact]
        public void CaseConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.SerializeToElement("hello world");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "case" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["case"] = "upper"
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("HELLO WORLD", result);
        }
    }
}
