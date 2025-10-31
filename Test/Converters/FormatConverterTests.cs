using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class FormatConverterTests
    {

        [Fact]
        public void FormatConverter_ShouldFormatNumber()
        {
            var converter = new FormatConverter();
            var value = JsonSerializer.SerializeToElement(123.456);
            var rule = new MappingRule
            {
                Conversion = new List<string> { "format" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["format"] = "F2"
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("123.46", result);
        }

        [Fact]
        public void FormatConverter_ShouldFormatCurrency()
        {
            var converter = new FormatConverter();
            var value = JsonSerializer.SerializeToElement(1234.56);
            var rule = new MappingRule
            {
                Conversion = new List<string> { "format" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["format"] = "C"
                }
            };

            var result = converter.Convert(value, rule, null!);

            // Using invariant culture may produce currency symbol, just check for number
            Assert.Contains("1", result!.ToString());
            Assert.Contains("234", result.ToString());
            Assert.Contains("56", result.ToString());
        }

        [Fact]
        public void FormatConverter_ShouldFormatDate()
        {
            var converter = new FormatConverter();
            var value = JsonSerializer.SerializeToElement("2023-01-15T10:30:00");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "format" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["format"] = "yyyy-MM-dd"
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("2023-01-15", result);
        }

        [Fact]
        public void FormatConverter_ShouldReturnRawTextForInvalidInput()
        {
            var converter = new FormatConverter();
            var value = JsonSerializer.SerializeToElement("not a date or number");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "format" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["format"] = "yyyy-MM-dd"
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("\"not a date or number\"", result);
        }

        [Fact]
        public void FormatConverter_ShouldHandleEmptyFormat()
        {
            var converter = new FormatConverter();
            var value = JsonSerializer.SerializeToElement(123.456);
            var rule = new MappingRule
            {
                Conversion = new List<string> { "format" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["format"] = ""
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("123.456", result);
        }
    }
}
