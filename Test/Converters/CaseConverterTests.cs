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
            var conversionParams = JsonSerializer.SerializeToElement(new { @case = "upper" });

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("HELLO WORLD", result);
        }

        [Fact]
        public void CaseConverter_ShouldConvertToCamelCase()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.SerializeToElement("hello world test");
            var conversionParams = JsonSerializer.SerializeToElement(new { @case = "camel" });

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("helloWorldTest", result);
        }

        [Fact]
        public void CaseConverter_ShouldConvertToPascalCase()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.SerializeToElement("hello world test");
            var conversionParams = JsonSerializer.SerializeToElement(new { @case = "pascal" });

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("HelloWorldTest", result);
        }

        [Fact]
        public void CaseConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new CaseConverter();
            var value = JsonSerializer.SerializeToElement("hello world");
            var conversionParams = JsonSerializer.SerializeToElement("upper");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("HELLO WORLD", result);
        }
    }
}
