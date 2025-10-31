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

            // 支持两种参数格式：
            // 1. 简化格式：直接是字符串值 "both" - 只指定 type
            // 2. 完整格式：对象 {"type": "both", "chars": "."}
            string trimType = "both";
            string? trimChars = null;
            
            if (conversionParams.ValueKind == JsonValueKind.String)
            {
                // 简化格式：直接是字符串值，表示 type
                trimType = conversionParams.GetString()?.ToLowerInvariant() ?? "both";
            }
            else if (conversionParams.ValueKind == JsonValueKind.Object)
            {
                // 完整格式：嵌套对象
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