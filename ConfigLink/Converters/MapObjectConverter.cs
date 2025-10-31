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
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            if (value.ValueKind != JsonValueKind.Object) return null;

            var subRules = DeserializeSubRules(conversionParams);
            return engine.Process(value, subRules);
        }
        List<MappingRule> DeserializeSubRules(JsonElement conversionParams)
        {
            var json = conversionParams.GetRawText();
            return JsonSerializer.Deserialize<List<MappingRule>>(json)!;
        }
    }
}
