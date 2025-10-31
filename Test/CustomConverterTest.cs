using System;
using System.Collections.Generic;
using System.Text.Json;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests
{
    public class CustomConverterTest
    {
        // Mock converter for testing
        private class TestConverter : IConverter
        {
            public string TestValue { get; }
            
            public TestConverter(string testValue = "test_result")
            {
                TestValue = testValue;
            }
            
            public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
            {
                return TestValue;
            }
        }

        [Fact]
        public void RegisterConverter_ValidNameAndConverter_ShouldRegisterSuccessfully()
        {
            // Arrange
            var rules = new List<MappingRule>();
            var engine = new MappingEngine(rules);
            var converter = new TestConverter();

            // Act & Assert - Should not throw
            engine.RegisterConverter("test_converter", converter);
        }

        [Fact]
        public void RegisterConverter_NullName_ShouldThrowArgumentException()
        {
            // Arrange
            var rules = new List<MappingRule>();
            var engine = new MappingEngine(rules);
            var converter = new TestConverter();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                engine.RegisterConverter(null!, converter));
            Assert.Equal("name", exception.ParamName);
            Assert.Contains("转换器名称不能为空", exception.Message);
        }

        [Fact]
        public void RegisterConverter_EmptyName_ShouldThrowArgumentException()
        {
            // Arrange
            var rules = new List<MappingRule>();
            var engine = new MappingEngine(rules);
            var converter = new TestConverter();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                engine.RegisterConverter("", converter));
            Assert.Equal("name", exception.ParamName);
            Assert.Contains("转换器名称不能为空", exception.Message);
        }

        [Fact]
        public void RegisterConverter_WhitespaceOnlyName_ShouldThrowArgumentException()
        {
            // Arrange
            var rules = new List<MappingRule>();
            var engine = new MappingEngine(rules);
            var converter = new TestConverter();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                engine.RegisterConverter("   ", converter));
            Assert.Equal("name", exception.ParamName);
            Assert.Contains("转换器名称不能为空", exception.Message);
        }

        [Fact]
        public void RegisterConverter_NullConverter_ShouldThrowArgumentNullException()
        {
            // Arrange
            var rules = new List<MappingRule>();
            var engine = new MappingEngine(rules);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                engine.RegisterConverter("test_converter", null!));
            Assert.Equal("converter", exception.ParamName);
        }

        [Fact]
        public void RegisterConverter_OverwriteExistingConverter_ShouldReplaceConverter()
        {
            // Arrange
            var rules = new List<MappingRule>
            {
                new MappingRule 
                { 
                    Source = "test_field", 
                    Target = "result", 
                    Conversion = new List<string> { "custom_converter" } 
                }
            };
            var engine = new MappingEngine(rules);
            
            var firstConverter = new TestConverter("first_result");
            var secondConverter = new TestConverter("second_result");
            
            var sourceObj = new { test_field = "input" };

            // Act
            engine.RegisterConverter("custom_converter", firstConverter);
            var firstResult = engine.Transform(sourceObj);
            
            engine.RegisterConverter("custom_converter", secondConverter);
            var secondResult = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("first_result", firstResult["result"]);
            Assert.Equal("second_result", secondResult["result"]);
        }

        [Fact]
        public void RegisterConverter_CustomConverterUsedInTransformation_ShouldApplyCustomLogic()
        {
            // Arrange
            var rules = new List<MappingRule>
            {
                new MappingRule 
                { 
                    Source = "input_field", 
                    Target = "output_field", 
                    Conversion = new List<string> { "my_custom_converter" } 
                }
            };
            var engine = new MappingEngine(rules);
            var customConverter = new TestConverter("custom_transformed_value");
            
            engine.RegisterConverter("my_custom_converter", customConverter);
            
            var sourceObj = new { input_field = "original_value" };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("custom_transformed_value", result["output_field"]);
        }

        [Fact]
        public void RegisterConverter_MultipleCustomConverters_ShouldRegisterAllIndependently()
        {
            // Arrange
            var rules = new List<MappingRule>
            {
                new MappingRule 
                { 
                    Source = "field1", 
                    Target = "result1", 
                    Conversion = new List<string> { "converter1" } 
                },
                new MappingRule 
                { 
                    Source = "field2", 
                    Target = "result2", 
                    Conversion = new List<string> { "converter2" } 
                }
            };
            var engine = new MappingEngine(rules);
            
            var converter1 = new TestConverter("result_from_converter1");
            var converter2 = new TestConverter("result_from_converter2");
            
            // Act
            engine.RegisterConverter("converter1", converter1);
            engine.RegisterConverter("converter2", converter2);
            
            var sourceObj = new { field1 = "input1", field2 = "input2" };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("result_from_converter1", result["result1"]);
            Assert.Equal("result_from_converter2", result["result2"]);
        }

        [Fact]
        public void RegisterConverter_OverwriteBuiltInConverter_ShouldReplaceBuiltInBehavior()
        {
            // Arrange
            var rules = new List<MappingRule>
            {
                new MappingRule 
                { 
                    Source = "test_field", 
                    Target = "result", 
                    Conversion = new List<string> { "trim" } // Built-in converter
                }
            };
            var engine = new MappingEngine(rules);
            var customTrimConverter = new TestConverter("custom_trim_result");
            
            // Act
            engine.RegisterConverter("trim", customTrimConverter);
            
            var sourceObj = new { test_field = "  spaced text  " };
            var result = engine.Transform(sourceObj);

            // Assert
            // Should use custom converter instead of built-in trim
            Assert.Equal("custom_trim_result", result["result"]);
        }
    }
}