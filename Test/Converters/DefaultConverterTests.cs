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
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "null" });

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("default text", result);
        }
    }
}
