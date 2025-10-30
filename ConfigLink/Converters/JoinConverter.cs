using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ConfigLink.Converters
{
    public class JoinConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            var separator = rule.ConversionParams?["join"]?.ToString() ?? "";

            if (value.ValueKind == JsonValueKind.Array)
            {
                var items = new List<string>();
                foreach (var item in value.EnumerateArray())
                {
                    items.Add(item.ToString() ?? "");
                }
                return string.Join(separator, items);
            }

            if (value.ValueKind == JsonValueKind.Object)
            {
                var items = new List<string>();
                foreach (var property in value.EnumerateObject())
                {
                    items.Add(property.Value.ToString() ?? "");
                }
                return string.Join(separator, items);
            }

            return value.ToString();
        }
    }
}