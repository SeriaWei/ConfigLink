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
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""name"",
                        ""target"": ""userName"",
                        ""conversion"": [""case""],
                        ""conversion_params"": {
                            ""case"": {""case"": ""camel""}
                        }
                    }
                ]
            }";

            var sourceJson = @"{
                ""name"": ""hello world test""
            }";

            var engine = new MappingEngine(mappingJson);
            var result = engine.Transform(sourceJson);

            Assert.Equal("helloWorldTest", result["userName"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseTrimConverter()
        {
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""message"",
                        ""target"": ""cleanMessage"",
                        ""conversion"": [""trim""],
                        ""conversion_params"": {
                            ""trim"": {""type"": ""both""}
                        }
                    }
                ]
            }";

            var sourceJson = @"{
                ""message"": ""  hello world  ""
            }";

            var engine = new MappingEngine(mappingJson);
            var result = engine.Transform(sourceJson);

            Assert.Equal("hello world", result["cleanMessage"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseReplaceConverter()
        {
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""text"",
                        ""target"": ""modifiedText"",
                        ""conversion"": [""replace""],
                        ""conversion_params"": {
                            ""replace"": {
                                ""search"": ""world"",
                                ""replace"": ""universe""
                            }
                        }
                    }
                ]
            }";

            var sourceJson = @"{
                ""text"": ""hello world""
            }";

            var engine = new MappingEngine(mappingJson);
            var result = engine.Transform(sourceJson);

            Assert.Equal("hello universe", result["modifiedText"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseSubstringConverter()
        {
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""fullText"",
                        ""target"": ""shortText"",
                        ""conversion"": [""substring""],
                        ""conversion_params"": {
                            ""substring"": {
                                ""start"": ""0"",
                                ""length"": ""5""
                            }
                        }
                    }
                ]
            }";

            var sourceJson = @"{
                ""fullText"": ""hello world""
            }";

            var engine = new MappingEngine(mappingJson);
            var result = engine.Transform(sourceJson);

            Assert.Equal("hello", result["shortText"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseDefaultConverter()
        {
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""nullValue"",
                        ""target"": ""valueWithDefault"",
                        ""conversion"": [""default""],
                        ""conversion_params"": {
                            ""default"": {
                                ""value"": ""default text"",
                                ""condition"": ""null""
                            }
                        }
                    }
                ]
            }";

            var sourceJson = @"{
                ""nullValue"": null
            }";

            var engine = new MappingEngine(mappingJson);
            var result = engine.Transform(sourceJson);

            Assert.True(result.ContainsKey("valueWithDefault"));
            Assert.NotNull(result["valueWithDefault"]);
            Assert.Equal("default text", result["valueWithDefault"]?.ToString());
        }

        [Fact]
        public void MappingEngine_ShouldUseNumberConverter()
        {
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""stringNumber"",
                        ""target"": ""intNumber"",
                        ""conversion"": [""number""],
                        ""conversion_params"": {
                            ""number"": {""type"": ""int""}
                        }
                    }
                ]
            }";

            var sourceJson = @"{
                ""stringNumber"": ""123""
            }";

            var engine = new MappingEngine(mappingJson);
            var result = engine.Transform(sourceJson);

            Assert.Equal(123, result["intNumber"]);
        }

        [Fact]
        public void MappingEngine_ShouldUseBooleanConverter()
        {
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""status"",
                        ""target"": ""isActive"",
                        ""conversion"": [""boolean""],
                        ""conversion_params"": {
                            ""boolean"": {""output"": ""boolean""}
                        }
                    }
                ]
            }";

            var sourceJson = @"{
                ""status"": ""yes""
            }";

            var engine = new MappingEngine(mappingJson);
            var result = engine.Transform(sourceJson);

            Assert.Equal(true, result["isActive"]);
        }

        [Fact]
        public void MappingEngine_ShouldChainMultipleConverters()
        {
            var mappingJson = @"{
                ""mappings"": [
                    {
                        ""source"": ""rawText"",
                        ""target"": ""processedText"",
                        ""conversion"": [""trim"", ""case""],
                        ""conversion_params"": {
                            ""trim"": {""type"": ""both""},
                            ""case"": {""case"": ""pascal""}
                        }
                    }
                ]
            }";

            var sourceJson = @"{
                ""rawText"": ""  hello world test  ""
            }";

            var engine = new MappingEngine(mappingJson);
            var result = engine.Transform(sourceJson);

            Assert.Equal("HelloWorldTest", result["processedText"]);
        }
    }
}