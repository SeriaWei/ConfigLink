using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ConfigLink.Converters
{
    public class MapObjectConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            if (value.ValueKind != JsonValueKind.Object) return null;

            var subRules = DeserializeSubRules(rule.ConversionParams?["map_object"]);
            return engine.Process(value, subRules);
        }
        List<MappingRule> DeserializeSubRules(object? obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<List<MappingRule>>(json)!;
        }
    }
}
