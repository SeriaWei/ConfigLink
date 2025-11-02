using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Tests.MappingEngineTests
{
    public class MappingEngine_ErrorHandlingTests
    {
        [Fact]
        public void Transform_InvalidSourcePath_ShouldSkipMapping()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule { Source = "nonexistent.path", Target = "result1" },
                new MappingRule { Source = "valid", Target = "result2" }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { valid = "value" };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.False(result.ContainsKey("result1")); // nonexistent path should be skipped
            Assert.Equal("value", result["result2"]); // valid path should work
        }

        [Fact]
        public void Transform_ArrayIndexOutOfBounds_ShouldSkipMapping()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule { Source = "items[5]", Target = "outOfBounds" },
                new MappingRule { Source = "items[0]", Target = "valid" }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { items = new[] { "first", "second" } };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.False(result.ContainsKey("outOfBounds")); // out of bounds should be skipped
            Assert.Equal("first", result["valid"]); // valid index should work
        }
    }
}