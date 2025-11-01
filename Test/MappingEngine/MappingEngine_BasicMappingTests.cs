using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Tests
{
    public class MappingEngine_BasicMappingTests
    {
        private static readonly List<MappingRule> BasicMappingRules = new()
        {
            new MappingRule { Source = "name", Target = "username" },
            new MappingRule { Source = "age", Target = "userAge" }
        };

        [Fact]
        public void Transform_BasicMapping_ShouldMapCorrectly()
        {
            // Arrange
            var engine = new MappingEngine(BasicMappingRules);
            var sourceObj = new { name = "John", age = 30 };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("John", result["username"]);
            Assert.Equal(30.0, result["userAge"]);
        }

        [Fact]
        public void Transform_MissingSourceProperty_ShouldSkipMapping()
        {
            // Arrange
            var engine = new MappingEngine(BasicMappingRules);
            var sourceObj = new { name = "John" }; // missing age

            // Act
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("John", result["username"]);
            Assert.False(result.ContainsKey("userAge"));
        }

        [Fact]
        public void Transform_EmptyMappings_ShouldReturnEmptyDictionary()
        {
            // Arrange
            var mappingRules = new List<MappingRule>();
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { name = "test" };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Transform_EmptySourceJson_ShouldReturnEmptyDictionary()
        {
            // Arrange
            var engine = new MappingEngine(BasicMappingRules);
            var sourceObj = new { };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Empty(result);
        }
    }
}