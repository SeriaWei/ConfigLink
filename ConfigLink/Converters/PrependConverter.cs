using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.Text.Json;

namespace ConfigLink.Converters
{
    public class PrependConverter : IConverter
    {
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            var prefix = "";
            if (conversionParams.ValueKind == JsonValueKind.String)
            {
                prefix = conversionParams.GetString() ?? "";
            }
            else if (conversionParams.ValueKind == JsonValueKind.Object && conversionParams.TryGetProperty("prefix", out var prefixProp))
            {
                prefix = prefixProp.GetString() ?? "";
            }

            if(value.ValueKind == JsonValueKind.Null)
            {
                return prefix;
            }
            if (value.ValueKind != JsonValueKind.String)
            {
                return prefix + value.GetRawText();
            }
            return prefix + value.GetString();
        }
    }
}
