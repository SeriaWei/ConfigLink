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
            var conversionParams = JsonSerializer.SerializeToElement(new { type = "both" });

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("hello world", result);
        }

        [Fact]
        public void TrimConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new TrimConverter();
            var value = JsonSerializer.SerializeToElement("  hello world  ");
            var conversionParams = JsonSerializer.SerializeToElement("both");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("hello world", result);
        }
    }
}
