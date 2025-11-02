using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigLink.Converters
{
    public class TrimConverter : IConverter
    {
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText().Trim('"');
            if (text == null)
                return null;

            string trimType = "both";
            string? trimChars = null;
            
            if (conversionParams.ValueKind == JsonValueKind.String)
            {
                trimType = conversionParams.GetString()?.ToLowerInvariant() ?? "both";
            }
            else if (conversionParams.ValueKind == JsonValueKind.Object)
            {
                if (conversionParams.TryGetProperty("type", out var typeProperty))
                {
                    trimType = typeProperty.GetString()?.ToLowerInvariant() ?? "both";
                }
                if (conversionParams.TryGetProperty("chars", out var charsProperty))
                {
                    trimChars = charsProperty.GetString();
                }
            }

            if (!string.IsNullOrEmpty(trimChars))
            {
                var chars = trimChars.ToCharArray();
                return trimType switch
                {
                    "start" or "left" => text.TrimStart(chars),
                    "end" or "right" => text.TrimEnd(chars),
                    "both" or "all" => text.Trim(chars),
                    _ => text.Trim(chars)
                };
            }

            return trimType switch
            {
                "start" or "left" => text.TrimStart(),
                "end" or "right" => text.TrimEnd(),
                "both" or "all" => text.Trim(),
                _ => text.Trim()
            };
        }
    }
}