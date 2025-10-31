using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class BooleanConverterTests
    {

        [Fact]
        public void BooleanConverter_ShouldConvertStringToBoolean()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.SerializeToElement("yes");
            var conversionParams = JsonSerializer.SerializeToElement(new { output = "boolean" });

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal(true, result);
        }

        [Fact]
        public void BooleanConverter_ShouldConvertToYesNoFormat()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.SerializeToElement(true);
            var conversionParams = JsonSerializer.SerializeToElement(new { output = "yesno" });

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("yes", result);
        }

        [Fact]
        public void BooleanConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.SerializeToElement(true);
            var conversionParams = JsonSerializer.SerializeToElement("yesno");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("yes", result);
        }
    }
}
