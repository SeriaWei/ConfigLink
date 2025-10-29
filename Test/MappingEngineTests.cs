using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace ConfigLink.Tests
{
    public class MappingEngineTests
    {
        private const string BasicMappingJson = @"{
            ""mappings"": [
                {
                    ""source"": ""name"",
                    ""target"": ""username""
                },
                {
                    ""source"": ""age"",
                    ""target"": ""userAge""
                }
            ]
        }";

        private const string ComplexMappingJson = @"{
            ""mappings"": [
                {
                    ""source"": ""user.profile.name"",
                    ""target"": ""displayName""
                },
                {
                    ""source"": ""items[0]"",
                    ""target"": ""firstItem""
                },
                {
                    ""source"": ""nested.array[1].value"",
                    ""target"": ""secondValue""
                }
            ]
        }";

        private const string RootTargetMappingJson = @"{
            ""mappings"": [
                {
                    ""source"": ""data"",
                    ""target"": ""$root"",
                    ""conversion"": [""map_object""]
                }
            ]
        }";

        [Fact]
        public void Constructor_ValidJson_ShouldInitializeSuccessfully()
        {
            // Act & Assert
            var engine = new MappingEngine(BasicMappingJson);
            Assert.NotNull(engine);
        }

        [Fact]
        public void Constructor_InvalidJson_ShouldThrowException()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => new MappingEngine(invalidJson));
        }

        [Fact]
        public void Transform_BasicMapping_ShouldMapCorrectly()
        {
            // Arrange
            var engine = new MappingEngine(BasicMappingJson);
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
            var engine = new MappingEngine(BasicMappingJson);
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
            var engine = new MappingEngine(ComplexMappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {""source"": ""nonexistent.path"", ""target"": ""result1""},
                    {""source"": ""valid"", ""target"": ""result2""}
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {""source"": ""items[5]"", ""target"": ""outOfBounds""},
                    {""source"": ""items[0]"", ""target"": ""valid""}
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {""source"": ""stringVal"", ""target"": ""str""},
                    {""source"": ""intVal"", ""target"": ""int""},
                    {""source"": ""doubleVal"", ""target"": ""dbl""},
                    {""source"": ""boolVal"", ""target"": ""bool""},
                    {""source"": ""nullVal"", ""target"": ""null""}
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var emptyMappingJson = @"{""mappings"": []}";
            var engine = new MappingEngine(emptyMappingJson);
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
            var engine = new MappingEngine(BasicMappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {""source"": ""items[0].name"", ""target"": ""firstItemName""},
                    {""source"": ""data.users[2].profile.email"", ""target"": ""thirdUserEmail""},
                    {""source"": ""config.settings[0]"", ""target"": ""firstSetting""}
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""price"",
                        ""target"": ""formattedPrice"",
                        ""conversion"": [""format""],
                        ""conversion_params"": {
                            ""format"": ""F2""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""date"",
                        ""target"": ""formattedDate"",
                        ""conversion"": [""format""],
                        ""conversion_params"": {
                            ""format"": ""yyyy-MM-dd""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""name"",
                        ""target"": ""prefixedName"",
                        ""conversion"": [""prepend""],
                        ""conversion_params"": {
                            ""prepend"": ""Mr. ""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
            var sourceJson = @"{""name"": ""John""}";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Equal("Mr. \"John\"", result["prefixedName"]);
        }

        [Fact]
        public void Transform_ToArrayJoinConverter_ShouldJoinFields()
        {
            // Arrange
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""address"",
                        ""target"": ""fullAddress"",
                        ""conversion"": [""to_array"", ""join""],
                        ""conversion_params"": {
                            ""to_array"": [""street"", ""city"", ""state"", ""zip""],
                            ""join"": "", ""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""value"",
                        ""target"": ""processedValue"",
                        ""conversion"": [""prepend"", ""format""],
                        ""conversion_params"": {
                            ""prepend"": ""$ "",
                            ""format"": ""F2""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""name"",
                        ""target"": ""processedName"",
                        ""conversion"": [""unknown_converter""]
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""name"",
                        ""target"": ""simpleName""
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""name"",
                        ""target"": ""simpleName"",
                        ""conversion"": []
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""person"",
                        ""target"": ""fullName"",
                        ""conversion"": [""to_array"", ""join""],
                        ""conversion_params"": {
                            ""to_array"": [""first"", ""middle"", ""last""],
                            ""join"": "" ""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""users"",
                        ""target"": ""processedUsers"",
                        ""conversion"": [""map_array""],
                        ""conversion_params"": {
                            ""map_array"": [
                                {""source"": ""name"", ""target"": ""fullName""},
                                {""source"": ""email"", ""target"": ""contact""}
                            ]
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""text"",
                        ""target"": ""formatted"",
                        ""conversion"": [""format""],
                        ""conversion_params"": {
                            ""format"": ""invalid""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""nullable"",
                        ""target"": ""processed"",
                        ""conversion"": [""prepend""],
                        ""conversion_params"": {
                            ""prepend"": ""prefix: ""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
            var sourceJson = @"{""nullable"": null}";

            // Act
            var result = engine.Transform(sourceJson);

            // Assert
            Assert.Equal("prefix: null", result["processed"]);
        }

        [Fact]
        public void Transform_ComplexConversionChain_ShouldApplyAllSteps()
        {
            // Arrange
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""data"",
                        ""target"": ""complexResult"",
                        ""conversion"": [""to_array"", ""join""],
                        ""conversion_params"": {
                            ""to_array"": [""first"", ""second""],
                            ""join"": "" | ""
                        }
                    }
                ]
            }";
            var engine = new MappingEngine(mappingJson);
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