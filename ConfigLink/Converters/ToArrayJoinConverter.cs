using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.Text.Json;

namespace ConfigLink.Converters
{
    public class ToArrayJoinConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            if (value.ValueKind != JsonValueKind.Object) return null;

            var fields = rule.ConversionParams?["to_array"] as JsonElement?;
            var join = rule.ConversionParams?["join"]?.ToString() ?? "";

            var list = new List<string>();
            if (fields?.ValueKind == JsonValueKind.Array)
            {
                foreach (var f in fields.Value.EnumerateArray())
                {
                    var path = f.GetString()!;
                    var v = engine.GetValueByPath(value, path);
                    list.Add(v?.ToString() ?? "");
                }
            }
            return string.Join(join, list);
        }
    }
}
