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
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            var fmt = rule.ConversionParams?["format"]?.ToString() ?? "";

            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetDecimal().ToString(fmt, CultureInfo.InvariantCulture),
                JsonValueKind.String when DateTime.TryParse(value.GetString(), out var dt) => dt.ToString(fmt),
                _ => value.GetRawText()
            };
        }
    }
}
