using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Tests
{
    public class MappingEngine_ConstructorTests
    {
        private static readonly List<MappingRule> BasicMappingRules = new()
        {
            new MappingRule { Source = "name", Target = "username" },
            new MappingRule { Source = "age", Target = "userAge" }
        };

        [Fact]
        public void Constructor_ValidJson_ShouldInitializeSuccessfully()
        {
            // Act & Assert
            var engine = new MappingEngine(BasicMappingRules);
            Assert.NotNull(engine);
        }

        [Fact]
        public void Constructor_EmptyRules_ShouldInitializeSuccessfully()
        {
            // Arrange & Act
            var engine = new MappingEngine(new List<MappingRule>());
            
            // Assert
            Assert.NotNull(engine);
        }
    }
}