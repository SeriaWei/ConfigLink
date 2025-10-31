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

        [Fact]
        public void TrimConverter_ShouldTrimWhitespace()
        {
            var converter = new TrimConverter();
            var value = JsonSerializer.SerializeToElement("  hello world  ");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "trim" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["trim"] = new { type = "both" }
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello world", result);
        }

        [Fact]
        public void TrimConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new TrimConverter();
            var value = JsonSerializer.SerializeToElement("  hello world  ");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "trim" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["trim"] = "both"
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello world", result);
        }
    }
}
