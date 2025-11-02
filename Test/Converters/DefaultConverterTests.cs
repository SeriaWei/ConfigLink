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
        private readonly MappingEngine _engine = new MappingEngine(new List<MappingRule>());

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForNull()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement((string?)null);
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "null" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldReturnOriginalValueForNonNull()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement("original value");
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "null" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("original value", result);
        }

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForEmptyString()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement("");
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "empty" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldReturnOriginalForNonEmptyString()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement("actual value");
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "empty" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("actual value", result);
        }

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForNullOrEmpty()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement((string?)null);
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "nullorempty" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForEmptyStringWithNullOrEmptyCondition()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement("");
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "nullorempty" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForWhitespaceString()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement("   ");
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "whitespace" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForNullWithWhitespaceCondition()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement((string?)null);
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "nullorwhitespace" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldReturnDefaultForWhitespaceWithNullOrWhitespaceCondition()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement("   ");
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "nullorwhitespace" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldHandleNumberValues()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement(42);
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "null" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal(42m, result); // Numbers are returned as decimal
        }

        [Fact]
        public void DefaultConverter_ShouldHandleBooleanValues()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement(true);
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "null" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.True(result is bool && (bool)result);
        }

        [Fact]
        public void DefaultConverter_ShouldHandleArrayEmptyCheck()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement(new int[] { });
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "empty" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldHandleObjectEmptyCheck()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement(new { }); // Empty object
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "empty" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("default text", result);
        }

        [Fact]
        public void DefaultConverter_ShouldHandleNonEmptyArray()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement(new int[] { 1, 2, 3 });
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "empty" });

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("[1,2,3]", result); // Arrays are returned as raw JSON text when not converted
        }

        [Fact]
        public void DefaultConverter_ShouldHandleDefaultCondition()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement("some value");
            var conversionParams = JsonSerializer.SerializeToElement(new { value = "default text", condition = "invalid_condition" }); // Invalid condition should default to null check

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Equal("some value", result);
        }

        [Fact]
        public void DefaultConverter_ShouldHandleNullConditionWithoutParams()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement((string?)null);
            var conversionParams = JsonSerializer.SerializeToElement(new { }); // No parameters

            var result = converter.Convert(value, conversionParams, _engine);

            Assert.Null(result);
        }

        [Fact]
        public void DefaultConverter_ShouldHandleComplexDefaultValue()
        {
            var converter = new DefaultConverter();
            var value = JsonSerializer.SerializeToElement((string?)null);
            var conversionParams = JsonSerializer.SerializeToElement(new { value = new { complex = "default" }, condition = "null" });

            var result = converter.Convert(value, conversionParams, _engine);

            // When the default value is a complex object, it should be returned as JsonElement or object
            Assert.NotNull(result);
        }
    }
}
