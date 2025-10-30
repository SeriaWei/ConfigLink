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
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            var prefix = rule.ConversionParams?["prepend"]?.ToString() ?? "";
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
