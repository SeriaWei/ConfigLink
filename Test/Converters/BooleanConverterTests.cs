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
        private MappingRule CreateRule(string converterType, object? conversionParams = null)
        {
            var rule = new MappingRule
            {
                Conversion = new List<string> { converterType }
            };

            if (conversionParams != null)
            {
                var innerParamsJson = JsonSerializer.Serialize(conversionParams);
                var outerJson = $@"{{ ""{converterType.ToLowerInvariant()}"": {innerParamsJson} }}";
                
                rule.ConversionParams = JsonSerializer.Deserialize<Dictionary<string, object>>(outerJson);
            }

            return rule;
        }

        private MappingRule CreateSimplifiedRule(string converterName, string parameterValue)
        {
            var rule = new MappingRule
            {
                Target = "test",
                Source = "test",
                Conversion = new List<string> { converterName }
            };

            // Create simplified parameter format: {"converterName": "value"}
            var conversionParams = new Dictionary<string, object?>
            {
                [converterName] = parameterValue
            };

            var jsonString = JsonSerializer.Serialize(conversionParams);
            var jsonDoc = JsonDocument.Parse(jsonString);
            rule.ConversionParams = jsonDoc.RootElement.EnumerateObject()
                                         .ToDictionary(p => p.Name, p => (object)p.Value);

            return rule;
        }

        [Fact]
        public void BooleanConverter_ShouldConvertStringToBoolean()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.SerializeToElement("yes");
            var rule = CreateRule("boolean", new { output = "boolean" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal(true, result);
        }

        [Fact]
        public void BooleanConverter_ShouldConvertToYesNoFormat()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.SerializeToElement(true);
            var rule = CreateRule("boolean", new { output = "yesno" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("yes", result);
        }

        [Fact]
        public void BooleanConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new BooleanConverter();
            var value = JsonSerializer.SerializeToElement(true);
            var rule = CreateSimplifiedRule("boolean", "yesno");

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("yes", result);
        }
    }
}
