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
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            if (value.ValueKind != JsonValueKind.Object) return null;

            var list = new List<object?>();
            if (conversionParams.ValueKind == JsonValueKind.Array)
            {
                foreach (var f in conversionParams.EnumerateArray())
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