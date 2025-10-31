/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConfigLink;
using ConfigLink.Converters;
using Xunit;

namespace ConfigLink.Tests.Converters
{
    public class MapArrayConverterTests
    {

        private MappingEngine CreateTestEngine()
        {
            // Create a simple mapping engine for testing
            var mappingRules = new List<MappingRule>
            {
                new MappingRule { Source = "test", Target = "test" }
            };
            return new MappingEngine(mappingRules);
        }

        [Fact]
        public void MapArrayConverter_ShouldReturnNullForNonArray()
        {
            var converter = new MapArrayConverter();
            var value = JsonSerializer.SerializeToElement(new { not = "array" });
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_array" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_array"] = new object[0]
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.Null(result);
        }

        [Fact]
        public void MapArrayConverter_ShouldTransformArrayElements()
        {
            var converter = new MapArrayConverter();
            var value = JsonSerializer.SerializeToElement(new[] {
                new { id = 1, name = "John" },
                new { id = 2, name = "Jane" }
            });
            
            var subRules = new[]
            {
                new { source = "id", target = "userId" },
                new { source = "name", target = "userName" }
            };
            
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_array" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_array"] = subRules
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            Assert.IsType<List<Dictionary<string, object?>>>(result);
            
            var list = (List<Dictionary<string, object?>>)result!;
            Assert.Equal(2, list.Count);
            
            // Check first item
            Assert.True(list[0].ContainsKey("userId"));
            Assert.True(list[0].ContainsKey("userName"));
            Assert.Equal(1.0, list[0]["userId"]); // JSON numbers are deserialized as double
            Assert.Equal("John", list[0]["userName"]);
            
            // Check second item
            Assert.Equal(2.0, list[1]["userId"]);
            Assert.Equal("Jane", list[1]["userName"]);
        }

        [Fact]
        public void MapArrayConverter_ShouldHandleEmptyArray()
        {
            var converter = new MapArrayConverter();
            var value = JsonSerializer.SerializeToElement(new object[0]);
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_array" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_array"] = new[] { new { source = "test", target = "test" } }
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            Assert.IsType<List<Dictionary<string, object?>>>(result);
            
            var list = (List<Dictionary<string, object?>>)result!;
            Assert.Empty(list);
        }

        [Fact]
        public void MapArrayConverter_ShouldHandleComplexMapping()
        {
            var converter = new MapArrayConverter();
            var value = JsonSerializer.SerializeToElement(new[] {
                new {
                    product = new {
                        id = "P001",
                        name = "Laptop"
                    },
                    price = 999.99,
                    inStock = true
                }
            });
            
            var subRules = new[]
            {
                new { source = "product.id", target = "productId" },
                new { source = "product.name", target = "productName" },
                new { source = "price", target = "cost" },
                new { source = "inStock", target = "available" }
            };
            
            var rule = new MappingRule
            {
                Conversion = new List<string> { "map_array" },
                ConversionParams = new Dictionary<string, object>
                {
                    ["map_array"] = subRules
                }
            };
            var engine = CreateTestEngine();

            var result = converter.Convert(value, rule, engine);

            Assert.NotNull(result);
            var list = (List<Dictionary<string, object?>>)result!;
            Assert.Single(list);
            
            var item = list[0];
            Assert.Equal("P001", item["productId"]);
            Assert.Equal("Laptop", item["productName"]);
            Assert.Equal(999.99, item["cost"]);
            Assert.Equal(true, item["available"]);
        }
    }
}
*/
