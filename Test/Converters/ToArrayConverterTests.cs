using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class ToArrayConverterTests
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
                        // 对于数组参数，转换为JsonElement
                        if (value.GetType().IsArray || value.GetType().Name.Contains("List"))
                        {
                            var jsonElement = JsonSerializer.SerializeToElement(value);
                            rule.ConversionParams[prop.Name] = jsonElement;
                        }
                        else
                        {
                            rule.ConversionParams[prop.Name] = value;
                        }
                    }
                }
            }

            return rule;
        }

        private MappingEngine CreateTestEngine()
        {
            // Create a simple mapping engine for testing
            var mappingRules = new List<MappingRule>
            {
                new MappingRule { Source = "test", Target = "test" }
            };
            return new MappingEngine(mappingRules);
        }

        [Fact]
        public void ToArrayConverter_ShouldReturnNullForNonObject()
        {
            var converter = new ToArrayConverter();
            var value = JsonSerializer.SerializeToElement(new[] { "not", "object" });
            var rule = CreateRule("to_array", new { to_array = new[] { "field1", "field2" } });
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.Null(result);
        }

        [Fact]
        public void ToArrayConverter_ShouldExtractFieldsToArray()
        {
            var converter = new ToArrayConverter();
            var value = JsonSerializer.SerializeToElement(new {
                street = "123 Main St",
                city = "Boston",
                state = "MA",
                zip = "02108",
                country = "USA"
            });
            
            var rule = CreateRule("to_array", new { to_array = new[] { "street", "city", "state", "zip" } });
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            Assert.IsType<List<object?>>(result);
            
            var list = (List<object?>)result!;
            Assert.Equal(4, list.Count);
            
            // Check each extracted value
            Assert.Equal("123 Main St", ((JsonElement)list[0]!).GetString());
            Assert.Equal("Boston", ((JsonElement)list[1]!).GetString());
            Assert.Equal("MA", ((JsonElement)list[2]!).GetString());
            Assert.Equal("02108", ((JsonElement)list[3]!).GetString());
        }

        [Fact]
        public void ToArrayConverter_ShouldHandleMissingFields()
        {
            var converter = new ToArrayConverter();
            var value = JsonSerializer.SerializeToElement(new {
                name = "John",
                age = 30
            });
            
            var rule = CreateRule("to_array", new { to_array = new[] { "name", "missing", "age", "another_missing" } });
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            var list = (List<object?>)result!;
            Assert.Equal(4, list.Count);
            
            // Existing fields should have values
            Assert.Equal("John", ((JsonElement)list[0]!).GetString());
            Assert.Equal(30, ((JsonElement)list[2]!).GetInt32());
            
            // Missing fields should be null
            Assert.Null(list[1]);
            Assert.Null(list[3]);
        }

        [Fact]
        public void ToArrayConverter_ShouldHandleNestedFields()
        {
            var converter = new ToArrayConverter();
            var value = JsonSerializer.SerializeToElement(new {
                user = new {
                    profile = new {
                        firstName = "John",
                        lastName = "Doe"
                    },
                    contact = new {
                        email = "john@example.com"
                    }
                },
                id = 12345
            });
            
            var rule = CreateRule("to_array", new { to_array = new[] { "user.profile.firstName", "user.profile.lastName", "user.contact.email", "id" } });
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            var list = (List<object?>)result!;
            Assert.Equal(4, list.Count);
            
            Assert.Equal("John", ((JsonElement)list[0]!).GetString());
            Assert.Equal("Doe", ((JsonElement)list[1]!).GetString());
            Assert.Equal("john@example.com", ((JsonElement)list[2]!).GetString());
            Assert.Equal(12345, ((JsonElement)list[3]!).GetInt32());
        }

        [Fact]
        public void ToArrayConverter_ShouldHandleEmptyFieldsList()
        {
            var converter = new ToArrayConverter();
            var value = JsonSerializer.SerializeToElement(new {
                name = "John",
                age = 30
            });
            
            var rule = CreateRule("to_array", new { to_array = new string[0] });
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            var list = (List<object?>)result!;
            Assert.Empty(list);
        }

        [Fact]
        public void ToArrayConverter_ShouldHandleArrayIndices()
        {
            var converter = new ToArrayConverter();
            var value = JsonSerializer.SerializeToElement(new {
                items = new[] { "first", "second", "third" },
                data = new {
                    values = new[] { 100, 200, 300 }
                }
            });
            
            var rule = CreateRule("to_array", new { to_array = new[] { "items[0]", "items[2]", "data.values[1]" } });
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            var list = (List<object?>)result!;
            Assert.Equal(3, list.Count);
            
            Assert.Equal("first", ((JsonElement)list[0]!).GetString());
            Assert.Equal("third", ((JsonElement)list[1]!).GetString());
            Assert.Equal(200, ((JsonElement)list[2]!).GetInt32());
        }
    }
}
