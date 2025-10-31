using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class ReplaceConverterTests
    {

        [Fact]
        public void ReplaceConverter_ShouldReplaceText()
        {
            var converter = new ReplaceConverter();
            var value = JsonSerializer.SerializeToElement("hello world");
            var rule = new MappingRule
            {
                Conversion = new List<string> { "replace" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["replace"] = new { from = "world", to = "universe" }
                }
            };

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("hello universe", result);
        }
    }
}
