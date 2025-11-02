using System;
using System.Text.Json;
using ConfigLink.Extensions;
using Xunit;

namespace ConfigLink.Tests.Extensions
{
    public class JsonElementExtensionsTests
    {
        [Fact]
        public void GetByPath_ShouldReturnElementForSimplePath()
        {
            var json = """{"name": "John", "age": 30}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetByPath("name");

            Assert.NotNull(result);
            Assert.Equal("John", result?.GetString());
        }

        [Fact]
        public void GetByPath_ShouldReturnElementForNestedPath()
        {
            var json = """{"user": {"profile": {"name": "John"}}}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetByPath("user.profile.name");

            Assert.NotNull(result);
            Assert.Equal("John", result?.GetString());
        }

        [Fact]
        public void GetByPath_ShouldReturnElementForArrayIndex()
        {
            var json = """{"items": ["first", "second", "third"]}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetByPath("items[1]");

            Assert.NotNull(result);
            Assert.Equal("second", result?.GetString());
        }

        [Fact]
        public void GetByPath_ShouldReturnElementForNestedArrayPath()
        {
            var json = """{"data": {"list": [{"name": "John"}, {"name": "Jane"}]}}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetByPath("data.list[1].name");

            Assert.NotNull(result);
            Assert.Equal("Jane", result?.GetString());
        }

        [Fact]
        public void GetByPath_ShouldReturnNullForInvalidPath()
        {
            var json = """{"name": "John", "age": 30}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetByPath("invalid.path");

            Assert.Null(result);
        }

        [Fact]
        public void GetByPath_ShouldReturnNullForInvalidArrayIndex()
        {
            var json = """{"items": ["first", "second"]}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetByPath("items[5]");

            Assert.Null(result);
        }

        [Fact]
        public void GetByPathValue_ShouldReturnStringForSimplePath()
        {
            var json = """{"name": "John", "age": 30}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetValueByPath("name");

            Assert.Equal("John", result);
        }

        [Fact]
        public void GetByPathValue_ShouldReturnStringForNumber()
        {
            var json = """{"name": "John", "age": 30}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetValueByPath("age");

            Assert.Equal("30", result);
        }

        [Fact]
        public void GetByPathValue_ShouldReturnStringForNestedPath()
        {
            var json = """{"user": {"profile": {"name": "John", "active": true}}}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetValueByPath("user.profile.active");

            Assert.Equal("True", result);
        }

        [Fact]
        public void GetByPathValue_ShouldReturnStringForArrayElement()
        {
            var json = """{"items": ["first", "second", "third"]}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetValueByPath("items[0]");

            Assert.Equal("first", result);
        }

        [Fact]
        public void GetByPathValue_ShouldReturnNullForNonExistentPath()
        {
            var json = """{"name": "John", "age": 30}""";
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            var result = element.GetValueByPath("invalid.path");

            Assert.Null(result);
        }
    }
}