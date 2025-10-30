using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ConfigLink.Converters
{

    public class MapArrayConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            if (value.ValueKind != JsonValueKind.Array) return null;

            var subRules = DeserializeSubRules(rule.ConversionParams?["map_array"]);
            var result = new List<Dictionary<string, object?>>();

            foreach (var item in value.EnumerateArray())
            {
                var dict = engine.Process(item, subRules);
                result.Add(dict);
            }
            return result;
        }
        List<MappingRule> DeserializeSubRules(object? obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<List<MappingRule>>(json)!;
        }
    }
}
