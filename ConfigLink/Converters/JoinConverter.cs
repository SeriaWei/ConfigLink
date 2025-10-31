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
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            var separator = "";
            if (conversionParams.ValueKind == JsonValueKind.String)
            {
                separator = conversionParams.GetString() ?? "";
            }
            else if (conversionParams.ValueKind == JsonValueKind.Object && conversionParams.TryGetProperty("separator", out var sepProp))
            {
                separator = sepProp.GetString() ?? "";
            }

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