using System;
using System.Collections.Generic;
using System.Text.Json;
using ConfigLink;
using Xunit;

namespace ConfigLink.Tests
{
    public class MappingEngineConverterIntegrationTests
    {

        [Fact]
        public void MappingEngine_ShouldUseCaseConverter()
        {
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "name",
                    Target = "userName",
                    Conversion = new List<string> { "case" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        ["case"] = new { @case = "camel" }
                    }
                }
            };

            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { name = "hello world test" };

            var result = engine.Transform(sourceObj);

            Assert.Equal("helloWorldTest", result["userName"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseTrimConverter()
        {
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "message",
                    Target = "cleanMessage",
                    Conversion = new List<string> { "trim" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        ["trim"] = new { type = "both" }
                    }
                }
            };

            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { message = "  hello world  " };

            var result = engine.Transform(sourceObj);

            Assert.Equal("hello world", result["cleanMessage"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseReplaceConverter()
        {
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "text",
                    Target = "modifiedText",
                    Conversion = new List<string> { "replace" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        ["replace"] = new {
                            search = "world",
                            replace = "universe"
                        }
                    }
                }
            };

            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { text = "hello world" };

            var result = engine.Transform(sourceObj);

            Assert.Equal("hello universe", result["modifiedText"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseSubstringConverter()
        {
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "fullText",
                    Target = "shortText",
                    Conversion = new List<string> { "substring" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        ["substring"] = new
                        {
                            start = "0",
                            length = "5"
                        }
                    }
                }
            };

            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { fullText = "hello world" };

            var result = engine.Transform(sourceObj);

            Assert.Equal("hello", result["shortText"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseDefaultConverter()
        {
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "nullValue",
                    Target = "valueWithDefault",
                    Conversion = new List<string> { "default" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        ["default"] = new
                        {
                            value = "default text",
                            condition = "null"
                        }
                    }
                }
            };

            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { nullValue = default(object) };

            var result = engine.Transform(sourceObj);

            Assert.True(result.ContainsKey("valueWithDefault"));
            Assert.NotNull(result["valueWithDefault"]);
            Assert.Equal("default text", result["valueWithDefault"]?.ToString());
        }

        [Fact]
        public void MappingEngine_ShouldUseNumberConverter()
        {
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "stringNumber",
                    Target = "intNumber",
                    Conversion = new List<string> { "number" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        ["number"] = new { type = "int" }
                    }
                }
            };

            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { stringNumber = "123" };

            var result = engine.Transform(sourceObj);

            Assert.Equal(123, result["intNumber"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseBooleanConverter()
        {
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "status",
                    Target = "isActive",
                    Conversion = new List<string> { "boolean" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        ["boolean"] = new { output = "boolean" }
                    }
                }
            };

            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { status = "yes" };

            var result = engine.Transform(sourceObj);

            Assert.Equal(true, result["isActive"]);
        }

        [Fact]
        public void MappingEngine_ShouldChainMultipleConverters()
        {
            var mappingRules = new List<MappingRule>
            {
                new MappingRule
                {
                    Source = "rawText",
                    Target = "processedText",
                    Conversion = new List<string> { "trim", "case" },
                    ConversionParams = new Dictionary<string, object>
                    {
                        ["trim"] = new { type = "both" },
                        ["case"] = new { @case = "pascal" }
                    }
                }
            };

            var engine = new MappingEngine(mappingRules);
            var sourceObj = new { rawText = "  hello world test  " };

            var result = engine.Transform(sourceObj);

            Assert.Equal("HelloWorldTest", result["processedText"]);
        }
    }
}

