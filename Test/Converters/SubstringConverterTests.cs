using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class SubstringConverterTests
    {

        [Fact]
        public void SubstringConverter_ShouldExtractSubstring()
        {
            var converter = new SubstringConverter();
            var value = JsonSerializer.SerializeToElement("hello world");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "substring" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["substring"] = new { start = 0, length = 5 }
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello", result);
        }
    }
}
