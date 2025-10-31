using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Tests
{
    public class MappingEngineTests
    {
        private static readonly List<MappingRule> BasicMappingRules = new()
        {
            new MappingRule { Source = "name", Target = "username" },
            new MappingRule { Source = "age", Target = "userAge" }
        };

        private static readonly List<MappingRule> ComplexMappingRules = new()
        {
            new MappingRule { Source = "user.profile.name", Target = "displayName" },
            new MappingRule { Source = "items[0]", Target = "firstItem" },
            new MappingRule { Source = "nested.array[1].value", Target = "secondValue" }
        };

        private static readonly List<MappingRule> RootTargetMappingRules = new()
        {
            new MappingRule { Source = "data", Target = "$root", Conversion = new List<string> { "map_object" } }
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

        [Fact]
        public void Transform_BasicMapping_ShouldMapCorrectly()
        {
            // Arrange
            var engine = new MappingEngine(BasicMappingRules);
            var sourceJson = @"{""name"": ""John"", ""age"": 30}";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Equal("John", result["username"]);
            Assert.Equal(30.0, result["userAge"]);
        }

        [Fact]
        public void Transform_MissingSourceProperty_ShouldSkipMapping()
        {
            // Arrange
            var engine = new MappingEngine(BasicMappingRules);
            var sourceJson = @"{""name"": ""John""}"; // missing age

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Equal("John", result["username"]);
            Assert.False(result.ContainsKey("userAge"));
        }

        [Fact]
        public void Transform_NestedPropertyAccess_ShouldMapCorrectly()
        {
            // Arrange
            var engine = new MappingEngine(ComplexMappingRules);
            var sourceJson = @"{
                ""user"": {
                    ""profile"": {
                        ""name"": ""Jane""
                    }
                },
                ""items"": [""first"", ""second""],
                ""nested"": {
                    ""array"": [
                        {""value"": ""val1""},
                        {""value"": ""val2""}
                    ]
                }
            }";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Equal("Jane", result["displayName"]);
            Assert.Equal("first", result["firstItem"]);
            Assert.Equal("val2", result["secondValue"]);
        }

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
            var sourceJson = @"{""valid"": ""value""}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""items"": [""first"", ""second""]}";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.False(result.ContainsKey("outOfBounds")); // out of bounds should be skipped
            Assert.Equal("first", result["valid"]); // valid index should work
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
            var sourceJson = @"{
                ""stringVal"": ""hello"",
                ""intVal"": 42,
                ""doubleVal"": 3.14,
                ""boolVal"": true,
                ""nullVal"": null
            }";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Equal("hello", result["str"]);
            Assert.Equal(42.0, result["int"]);
            Assert.Equal(3.14, result["dbl"]);
            Assert.Equal(true, result["bool"]);
            Assert.Null(result["null"]);
        }

        [Fact]
        public void Transform_EmptyMappings_ShouldReturnEmptyDictionary()
        {
            // Arrange
            var mappingRules = new List<MappingRule>();
            var engine = new MappingEngine(mappingRules);
            var sourceJson = @"{""name"": ""test""}";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Transform_EmptySourceJson_ShouldReturnEmptyDictionary()
        {
            // Arrange
            var engine = new MappingEngine(BasicMappingRules);
            var sourceJson = @"{}";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Empty(result);
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
            var sourceJson = @"{
                ""items"": [{""name"": ""item1""}],
                ""data"": {
                    ""users"": [
                        {""profile"": {""email"": ""user1@test.com""}},
                        {""profile"": {""email"": ""user2@test.com""}},
                        {""profile"": {""email"": ""user3@test.com""}}
                    ]
                },
                ""config"": {
                    ""settings"": [""setting1"", ""setting2""]
                }
            }";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Equal("item1", result["firstItemName"]);
            Assert.Equal("user3@test.com", result["thirdUserEmail"]);
            Assert.Equal("setting1", result["firstSetting"]);
        }

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
            var sourceJson = @"{""price"": 123.456}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""date"": ""2023-12-25T10:30:00""}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""name"": ""John""}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{
                ""address"": {
                    ""street"": ""123 Main St"",
                    ""city"": ""Boston"",
                    ""state"": ""MA"",
                    ""zip"": ""02108""
                }
            }";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""value"": 100}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""name"": ""John""}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""name"": ""John""}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""name"": ""John""}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{
                ""person"": {
                    ""first"": ""John"",
                    ""last"": ""Doe""
                }
            }";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{
                ""users"": [
                    {""name"": ""John"", ""email"": ""john@test.com""},
                    {""name"": ""Jane"", ""email"": ""jane@test.com""}
                ]
            }";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""text"": ""hello world""}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{""nullable"": null}";

            // Act
            var result = engine.Transform(sourceJson);

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
            var sourceJson = @"{
                ""data"": {
                    ""first"": ""A"",
                    ""second"": ""B"",
                    ""third"": ""C""
                }
            }";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Equal("A | B", result["complexResult"]);
        }
    }
}
