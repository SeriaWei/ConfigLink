using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ConfigLink.Converters
{
    public class ToArrayConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            if (value.ValueKind != JsonValueKind.Object) return null;

            var fields = rule.ConversionParams?["to_array"];
            var fieldsElement = fields is JsonElement je ? je : JsonSerializer.SerializeToElement(fields);

            var list = new List<object?>();
            if (fieldsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var f in fieldsElement.EnumerateArray())
                {
                    var path = f.GetString()!;
                    var v = engine.GetValueByPath(value, path);
                    list.Add(v);
                }
            }
            return list;
        }
    }
}