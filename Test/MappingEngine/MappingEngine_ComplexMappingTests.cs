using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Tests.MappingEngineTests
{
    public class MappingEngine_ComplexMappingTests
    {
        private static readonly List<MappingRule> ComplexMappingRules = new()
        {
            new MappingRule { Source = "user.profile.name", Target = "displayName" },
            new MappingRule { Source = "items[0]", Target = "firstItem" },
            new MappingRule { Source = "nested.array[1].value", Target = "secondValue" }
        };

        [Fact]
        public void Transform_NestedPropertyAccess_ShouldMapCorrectly()
        {
            // Arrange
            var engine = new MappingEngine(ComplexMappingRules);
            var sourceObj = new
            {
                user = new
                {
                    profile = new
                    {
                        name = "Jane"
                    }
                },
                items = new[] { "first", "second" },
                nested = new
                {
                    array = new[]
                    {
                        new { value = "val1" },
                        new { value = "val2" }
                    }
                }
            };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("Jane", result["displayName"]);
            Assert.Equal("first", result["firstItem"]);
            Assert.Equal("val2", result["secondValue"]);
        }

        [Fact]
        public void Transform_ComplexNestedPaths_ShouldMapCorrectly()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule { Source = "items[0].name", Target = "firstItemName" },
                new MappingRule { Source = "data.users[2].profile.email", Target = "thirdUserEmail" },
                new MappingRule { Source = "config.settings[0]", Target = "firstSetting" }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new
            {
                items = new[] { new { name = "item1" } },
                data = new
                {
                    users = new[]
                    {
                        new { profile = new { email = "user1@test.com" } },
                        new { profile = new { email = "user2@test.com" } },
                        new { profile = new { email = "user3@test.com" } }
                    }
                },
                config = new
                {
                    settings = new[] { "setting1", "setting2" }
                }
            };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("item1", result["firstItemName"]);
            Assert.Equal("user3@test.com", result["thirdUserEmail"]);
            Assert.Equal("setting1", result["firstSetting"]);
        }

        [Fact]
        public void Transform_DifferentJsonValueTypes_ShouldMapCorrectly()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule { Source = "stringVal", Target = "str" },
                new MappingRule { Source = "intVal", Target = "int" },
                new MappingRule { Source = "doubleVal", Target = "dbl" },
                new MappingRule { Source = "boolVal", Target = "bool" },
                new MappingRule { Source = "nullVal", Target = "null" }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new
            {
                stringVal = "hello",
                intVal = 42,
                doubleVal = 3.14,
                boolVal = true,
                nullVal = default(object)
            };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("hello", result["str"]);
            Assert.Equal(42.0, result["int"]);
            Assert.Equal(3.14, result["dbl"]);
            Assert.Equal(true, result["bool"]);
            Assert.Null(result["null"]);
        }
    }
}