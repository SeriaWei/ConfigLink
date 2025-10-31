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
        private MappingRule CreateRule(string converterType, object? conversionParams = null)
        {
            var rule = new MappingRule
            {
                Conversion = new List<string> { converterType }
            };

            if (conversionParams != null)
            {
                rule.ConversionParams = new Dictionary<string, object>();
                
                // 使用反射获取参数对象的属�?
                var properties = conversionParams.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(conversionParams);
                    if (value != null)
                    {
                        rule.ConversionParams[prop.Name] = value;
                    }
                }
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
        public void JoinConverter_ShouldJoinArrayWithComma()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new[] { "apple", "banana", "cherry" });
            var rule = CreateRule("join", new { join = ", " });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("apple, banana, cherry", result);
        }

        [Fact]
        public void JoinConverter_ShouldJoinArrayWithCustomSeparator()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new[] { "one", "two", "three" });
            var rule = CreateRule("join", new { join = " | " });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("one | two | three", result);
        }

        [Fact]
        public void JoinConverter_ShouldJoinObjectValues()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new { a = "value1", b = "value2", c = "value3" });
            var rule = CreateRule("join", new { join = "-" });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("value1-value2-value3", result);
        }

        [Fact]
        public void JoinConverter_ShouldHandleEmptyArray()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new object[0]);
            var rule = CreateRule("join", new { join = ", " });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("", result);
        }

        [Fact]
        public void JoinConverter_ShouldReturnStringForNonArrayValue()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement("single value");
            var rule = CreateRule("join", new { join = ", " });

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("single value", result);
        }

        [Fact]
        public void JoinConverter_ShouldSupportSimplifiedFormat()
        {
            var converter = new JoinConverter();
            var value = JsonSerializer.SerializeToElement(new[] { "a", "b", "c" });
            var rule = CreateSimplifiedRule("join", ";");

            var result = converter.Convert(value, rule, null!);

            Assert.Equal("a;b;c", result);
        }
    }
}
