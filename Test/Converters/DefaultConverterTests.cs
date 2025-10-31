using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class DefaultConverterTests
    {

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForNull()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement((string?)null);
            var rule = new MappingRule
            {
                Conversion = new List<string> { "default" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["default"] = new { value = "default text", condition = "null" }
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("default text", result);
        }
    }
}
