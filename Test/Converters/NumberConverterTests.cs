using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class NumberConverterTests
    {

        [Fact]
        public void NumberConverter_ShouldConvertStringToInt()
        {
            var converter = new NumberConverter();
            var value = JsonSerializer.SerializeToElement("123");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "number" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["number"] = new { type = "int" }
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal(123, result);
        }

        [Fact]
        public void NumberConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new NumberConverter();
            var value = JsonSerializer.SerializeToElement("123");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "number" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["number"] = "int"
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal(123, result);
        }
    }
}
