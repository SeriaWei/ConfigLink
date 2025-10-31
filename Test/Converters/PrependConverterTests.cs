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
            var rule = new MappingRule
            {
                Conversion = new List<string> { "prepend" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["prepend"] = "Hello "
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("Hello world", result);
        }

        [Fact]
        public void PrependConverter_ShouldPrependToNumber()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement(123);
            var rule = new MappingRule
            {
                Conversion = new List<string> { "prepend" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["prepend"] = "Number: "
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("Number: 123", result);
        }

        [Fact]
        public void PrependConverter_ShouldHandleNullValue()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement((string?)null);
            var rule = new MappingRule
            {
                Conversion = new List<string> { "prepend" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["prepend"] = "Prefix"
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("Prefix", result);
        }

        [Fact]
        public void PrependConverter_ShouldPrependToArray()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement(new[] { 1, 2, 3 });
            var rule = new MappingRule
            {
                Conversion = new List<string> { "prepend" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["prepend"] = "Array: "
                }
            };

            var result = converter.Convert(value, rule, null!);

            // The actual result uses compact JSON format without spaces
            Assert.Equal("Array: [1,2,3]", result);
        }

        [Fact]
        public void PrependConverter_ShouldHandleEmptyPrefix()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement("test");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "prepend" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["prepend"] = ""
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("test", result);
        }

        [Fact]
        public void PrependConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new PrependConverter();
            var value = JsonSerializer.SerializeToElement("text");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "prepend" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["prepend"] = "PREFIX: "
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("PREFIX: text", result);
        }
    }
}
