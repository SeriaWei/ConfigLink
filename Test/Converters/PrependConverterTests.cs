using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class PrependConverterTests
    {

        [Fact]
        public void PrependConverter_ShouldPrependToString()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement("world");
            var conversionParams = JsonSerializer.SerializeToElement("Hello ");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("Hello world", result);
        }

        [Fact]
        public void PrependConverter_ShouldPrependToNumber()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement(123);
            var conversionParams = JsonSerializer.SerializeToElement("Number: ");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("Number: 123", result);
        }

        [Fact]
        public void PrependConverter_ShouldHandleNull()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement((string?)null);
            var conversionParams = JsonSerializer.SerializeToElement("Prefix");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("Prefix", result);
        }

        [Fact]
        public void PrependConverter_ShouldPrependToArray()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement(new[] { 1, 2, 3 });
            var conversionParams = JsonSerializer.SerializeToElement("Array: ");

            var result = converter.Convert(value, conversionParams, null!);

            // The actual result uses compact JSON format without spaces
            Assert.Equal("Array: [1,2,3]", result);
        }

        [Fact]
        public void PrependConverter_ShouldHandleEmptyPrefix()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement("test");
            var conversionParams = JsonSerializer.SerializeToElement("");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("test", result);
        }

        [Fact]
        public void PrependConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement("text");
            var conversionParams = JsonSerializer.SerializeToElement("PREFIX: ");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("PREFIX: text", result);
        }
    }
}
