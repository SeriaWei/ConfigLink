using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.Json;

namespace ConfigLink.Converters
{
    public class FormatConverter : IConverter
    {
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            var fmt = "";
            if (conversionParams.ValueKind == JsonValueKind.String)
            {
                fmt = conversionParams.GetString() ?? "";
            }
            else if (conversionParams.ValueKind == JsonValueKind.Object && conversionParams.TryGetProperty("format", out var formatProp))
            {
                fmt = formatProp.GetString() ?? "";
            }

            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetDecimal().ToString(fmt, CultureInfo.InvariantCulture),
                JsonValueKind.String when DateTime.TryParse(value.GetString(), out var dt) => dt.ToString(fmt),
                _ => value.GetRawText()
            };
        }
    }
}
