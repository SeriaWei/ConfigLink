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

            var sourceJson = @"{
                ""name"": ""hello world test""
            }";

            var engine = new MappingEngine(mappingRules);
            var result = engine.Transform(sourceJson);

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

            var sourceJson = @"{
                ""message"": ""  hello world  ""
            }";

            var engine = new MappingEngine(mappingRules);
            var result = engine.Transform(sourceJson);

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

            var sourceJson = @"{
                ""text"": ""hello world""
            }";

            var engine = new MappingEngine(mappingRules);
            var result = engine.Transform(sourceJson);

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

            var sourceJson = @"{
                ""fullText"": ""hello world""
            }";

            var engine = new MappingEngine(mappingRules);
            var result = engine.Transform(sourceJson);

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

            var sourceJson = @"{
                ""nullValue"": null
            }";

            var engine = new MappingEngine(mappingRules);
            var result = engine.Transform(sourceJson);

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

            var sourceJson = @"{
                ""stringNumber"": ""123""
            }";

            var engine = new MappingEngine(mappingRules);
            var result = engine.Transform(sourceJson);

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

            var sourceJson = @"{
                ""status"": ""yes""
            }";

            var engine = new MappingEngine(mappingRules);
            var result = engine.Transform(sourceJson);

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

            var sourceJson = @"{
                ""rawText"": ""  hello world test  ""
            }";

            var engine = new MappingEngine(mappingRules);
            var result = engine.Transform(sourceJson);

            Assert.Equal("HelloWorldTest", result["processedText"]);
        }
    }
}
