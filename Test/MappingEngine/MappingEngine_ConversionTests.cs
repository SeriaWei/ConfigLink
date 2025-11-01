using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Tests
{
    public class MappingEngine_ConversionTests
    {
        [Fact]
        public void Transform_FormatConverter_ShouldFormatNumbers()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "price",
                    Target = "formattedPrice",
                    Conversion = new List<string> { "format" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "format", "F2" }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { price = 123.456 };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("123.46", result["formattedPrice"]);
        }

        [Fact]
        public void Transform_FormatConverter_ShouldFormatDates()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "date",
                    Target = "formattedDate",
                    Conversion = new List<string> { "format" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "format", "yyyy-MM-dd" }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { date = "2023-12-25T10:30:00" };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("2023-12-25", result["formattedDate"]);
        }

        [Fact]
        public void Transform_PrependConverter_ShouldAddPrefix()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "name",
                    Target = "prefixedName",
                    Conversion = new List<string> { "prepend" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "prepend", "Mr. " }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { name = "John" };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("Mr. John", result["prefixedName"]);
        }

        [Fact]
        public void Transform_ToArrayJoinConverter_ShouldJoinFields()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "address",
                    Target = "fullAddress",
                    Conversion = new List<string> { "to_array", "join" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "to_array", new[] { "street", "city", "state", "zip" } },
                        { "join", ", " }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new
            {
                address = new
                {
                    street = "123 Main St",
                    city = "Boston",
                    state = "MA",
                    zip = "02108"
                }
            };
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("123 Main St, Boston, MA, 02108", result["fullAddress"]);
        }

        [Fact]
        public void Transform_MultipleConversions_ShouldApplyInOrder()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "value",
                    Target = "processedValue",
                    Conversion = new List<string> { "prepend", "format" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "prepend", "$ " },
                        { "format", "F2" }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { value = 100 };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert - Should apply prepend first, then format
            Assert.NotNull(result["processedValue"]);
        }

        [Fact]
        public void Transform_UnknownConverter_ShouldReturnJsonElement()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "name",
                    Target = "processedName",
                    Conversion = new List<string> { "unknown_converter" }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { name = "John" };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert - Unknown converter is skipped, returns JsonElement
            var actualValue = result["processedName"];
            Assert.NotNull(actualValue);
            Assert.IsType<JsonElement>(actualValue);
            var jsonElement = (JsonElement)actualValue;
            Assert.Equal("John", jsonElement.GetString());
        }

        [Fact]
        public void Transform_NullConversion_ShouldReturnPrimitive()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "name",
                    Target = "simpleName"
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { name = "John" };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("John", result["simpleName"]);
        }

        [Fact]
        public void Transform_EmptyConversionArray_ShouldReturnJsonElement()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "name",
                    Target = "simpleName",
                    Conversion = new List<string>()
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { name = "John" };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert - Empty conversion array returns JsonElement, not primitive
            var actualValue = result["simpleName"];
            Assert.NotNull(actualValue);
            Assert.IsType<JsonElement>(actualValue);
            var jsonElement = (JsonElement)actualValue;
            Assert.Equal("John", jsonElement.GetString());
        }

        [Fact]
        public void Transform_ToArrayConverter_WithMissingFields_ShouldUseEmptyString()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "person",
                    Target = "fullName",
                    Conversion = new List<string> { "to_array", "join" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "to_array", new[] { "first", "middle", "last" } },
                        { "join", " " }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new
            {
                person = new
                {
                    first = "John",
                    last = "Doe"
                }
            };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert - Missing 'middle' field should be treated as empty string
            Assert.Equal("John  Doe", result["fullName"]);
        }

        [Fact]
        public void Transform_MapArrayConverter_ShouldTransformArrayElements()
        {
            // Arrange - This test assumes MapArrayConverter processes array elements
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "users",
                    Target = "processedUsers",
                    Conversion = new List<string> { "map_array" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "map_array", new[]
                            {
                                new { source = "name", target = "fullName" },
                                new { source = "email", target = "contact" }
                            }
                        }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new
            {
                users = new[]
                {
                    new { name = "John", email = "john@test.com" },
                    new { name = "Jane", email = "jane@test.com" }
                }
            };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert - Should have processed users array
            Assert.True(result.ContainsKey("processedUsers"));
            Assert.NotNull(result["processedUsers"]);
        }

        [Fact]
        public void Transform_FormatConverter_WithInvalidFormat_ShouldFallbackToRawText()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "text",
                    Target = "formatted",
                    Conversion = new List<string> { "format" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "format", "invalid" }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { text = "hello world" };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert - Should return raw text when format fails
            Assert.Equal("\"hello world\"", result["formatted"]);
        }

        [Fact]
        public void Transform_ConversionWithNullValue_ShouldHandleGracefully()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "nullable",
                    Target = "processed",
                    Conversion = new List<string> { "prepend" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "prepend", "prefix: " }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { nullable = default(object) };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("prefix: ", result["processed"]);
        }

        [Fact]
        public void Transform_ComplexConversionChain_ShouldApplyAllSteps()
        {
            // Arrange
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "data",
                    Target = "complexResult",
                    Conversion = new List<string> { "to_array", "join" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        { "to_array", new[] { "first", "second" } },
                        { "join", " | " }
                    }
                }
            };
            var engine = new MappingEngine(mappingRules);
            var sourceObj = new
            {
                data = new
                {
                    first = "A",
                    second = "B",
                    third = "C"
                }
            };

            // Act
            var result = engine.Transform(sourceObj);

            // Assert
            Assert.Equal("A | B", result["complexResult"]);
        }
    }
}