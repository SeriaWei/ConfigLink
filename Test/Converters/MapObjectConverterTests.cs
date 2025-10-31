/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class MapObjectConverterTests
    {

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
        public void MapObjectConverter_ShouldReturnNullForNonObject()
        {
            var converter = new MapObjectConverter();
            var value = JsonSerializer.SerializeToElement(new[] { "not", "object" });
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_object" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_object"] = new object[0]
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.Null(result);
        }

        [Fact]
        public void MapObjectConverter_ShouldTransformObject()
        {
            var converter = new MapObjectConverter();
            var value = JsonSerializer.SerializeToElement(new {
                firstName = "John",
                lastName = "Doe",
                age = 30,
                email = "john.doe@example.com"
            });
            
            var subRules = new[]
            {
                new { source = "firstName", target = "first_name" },
                new { source = "lastName", target = "last_name" },
                new { source = "age", target = "user_age" },
                new { source = "email", target = "contact_email" }
            };
            
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_object" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_object"] = subRules
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            Assert.IsType<Dictionary<string, object?>>(result);
            
            var dict = (Dictionary<string, object?>)result!;
            Assert.Equal("John", dict["first_name"]);
            Assert.Equal("Doe", dict["last_name"]);
            Assert.Equal(30.0, dict["user_age"]); // JSON numbers are deserialized as double
            Assert.Equal("john.doe@example.com", dict["contact_email"]);
        }

        [Fact]
        public void MapObjectConverter_ShouldHandleNestedObjects()
        {
            var converter = new MapObjectConverter();
            var value = JsonSerializer.SerializeToElement(new {
                user = new {
                    profile = new {
                        name = "Jane Smith"
                    },
                    settings = new {
                        theme = "dark"
                    }
                },
                metadata = new {
                    created = "2023-01-01"
                }
            });
            
            var subRules = new[]
            {
                new { source = "user.profile.name", target = "displayName" },
                new { source = "user.settings.theme", target = "userTheme" },
                new { source = "metadata.created", target = "createdDate" }
            };
            
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_object" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_object"] = subRules
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            var dict = (Dictionary<string, object?>)result!;
            Assert.Equal("Jane Smith", dict["displayName"]);
            Assert.Equal("dark", dict["userTheme"]);
            Assert.Equal("2023-01-01", dict["createdDate"]);
        }

        [Fact]
        public void MapObjectConverter_ShouldHandleEmptyObject()
        {
            var converter = new MapObjectConverter();
            var value = JsonSerializer.SerializeToElement(new { });
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_object" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_object"] = new[] { new { source = "nonexistent", target = "missing" } }
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            Assert.IsType<Dictionary<string, object?>>(result);
            
            var dict = (Dictionary<string, object?>)result!;
            Assert.Empty(dict); // No mappings should succeed on empty object
        }

        [Fact]
        public void MapObjectConverter_ShouldSkipMissingProperties()
        {
            var converter = new MapObjectConverter();
            var value = JsonSerializer.SerializeToElement(new {
                name = "John",
                age = 25
            });
            
            var subRules = new[]
            {
                new { source = "name", target = "userName" },
                new { source = "nonexistent", target = "missing" },
                new { source = "age", target = "userAge" }
            };
            
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_object" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_object"] = subRules
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            var dict = (Dictionary<string, object?>)result!;
            
            // Should have mapped existing properties
            Assert.Equal("John", dict["userName"]);
            Assert.Equal(25.0, dict["userAge"]);
            
            // Should not have mapped missing property
            Assert.False(dict.ContainsKey("missing"));
        }
    }
}
*/
