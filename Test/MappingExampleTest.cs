using ConfigLink;
using System.Text.Json;
using System.Collections.Generic;
using Xunit;

namespace ConfigLink.Tests
{
    public class MappingExampleTest
    {
        private static List<MappingRule> ParseMappingJson(string json)
        {
            var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("mappings")
                      .Deserialize<List<MappingRule>>()!;
        }

        [Fact]
        public void TestFromJsonFile()
        {
            var engine = new MappingEngine(ParseMappingJson(File.ReadAllText("config/mapping.json")));

            var sourceJson = File.ReadAllText("config/sample_input.json");

            var result = engine.Transform(sourceJson);

            // Verify basic field mappings
            Assert.Equal(1001.0, result["id"]); // Numbers are converted to double
            Assert.Equal(501.0, result["customer_id"]);
            Assert.Equal("John Doe", result["customer_name"]);
            Assert.Equal("john@example.com", result["customer_email"]);
            
            // Verify formatted date
            Assert.Equal("2025-10-29", result["order_date"]);
            
            // Verify formatted currency
            Assert.Equal("$59.97", result["total_amount"]);
            
            // Verify shipping address fields (from $root expansion)
            Assert.Equal("123 Main St", result["shipping_street"]);
            Assert.Equal("Boston", result["shipping_city"]);
            Assert.Equal("MA", result["shipping_state"]);
            Assert.Equal("02108", result["shipping_zipcode"]);
            Assert.Equal("USA", result["shipping_country"]);
            
            // Verify first product name from array access
            Assert.Equal("First Item", result["product_name"]);
            
            // Verify billing address join
            Assert.Equal("456 Oak Ave, Springfield, IL, 62701, USA", result["billing_address"]);
            
            // Verify items array exists and has been processed
            Assert.True(result.ContainsKey("items"));
            Assert.NotNull(result["items"]);
            
            // The result should have all expected keys
            var expectedKeys = new[] { 
                "id", "customer_id", "customer_name", "customer_email", 
                "order_date", "total_amount", "items", 
                "shipping_street", "shipping_city", "shipping_state", "shipping_zipcode", "shipping_country",
                "product_name", "billing_address" 
            };
            
            foreach (var key in expectedKeys)
            {
                Assert.True(result.ContainsKey(key), $"Result should contain key: {key}");
            }
        }
    }
}
