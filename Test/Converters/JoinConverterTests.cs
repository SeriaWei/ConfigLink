using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class JoinConverterTests
    {

        [Fact]
        public void JoinConverter_ShouldJoinArrayWithComma()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new[] { "apple", "banana", "cherry" });
            var conversionParams = JsonSerializer.SerializeToElement(", ");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("apple, banana, cherry", result);
        }

        [Fact]
        public void JoinConverter_ShouldJoinArrayWithCustomSeparator()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new[] { "one", "two", "three" });
            var conversionParams = JsonSerializer.SerializeToElement(" | ");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("one | two | three", result);
        }

        [Fact]
        public void JoinConverter_ShouldJoinObjectValues()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new { a = "value1", b = "value2", c = "value3" });
            var conversionParams = JsonSerializer.SerializeToElement("-");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("value1-value2-value3", result);
        }

        [Fact]
        public void JoinConverter_ShouldHandleEmptyArray()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new object[0]);
            var conversionParams = JsonSerializer.SerializeToElement(", ");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("", result);
        }

        [Fact]
        public void JoinConverter_ShouldReturnStringForNonArrayValue()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement("single value");
            var conversionParams = JsonSerializer.SerializeToElement(", ");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("single value", result);
        }

        [Fact]
        public void JoinConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new[] { "a", "b", "c" });
            var conversionParams = JsonSerializer.SerializeToElement(";");

            var result = converter.Convert(value, conversionParams, null!);

            Assert.Equal("a;b;c", result);
        }
    }
}
