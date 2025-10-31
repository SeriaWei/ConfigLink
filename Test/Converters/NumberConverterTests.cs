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
            var conversionParams = JsonSerializer.SerializeToElement(new { type = "int" });

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal(123, result);
        }

        [Fact]
        public void NumberConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new NumberConverter();
            var value = JsonSerializer.SerializeToElement("123");
            var conversionParams = JsonSerializer.SerializeToElement("int");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal(123, result);
        }
    }
}
