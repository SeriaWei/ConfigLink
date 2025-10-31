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
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText().Trim('"');
            if (text == null)
                return null;

            // 支持两种参数格式：
            // 1. 简化格式：{"trim": "both"} - 只指定 type
            // 2. 完整格式：{"trim": {"type": "both", "chars": "."}}
            string trimType = "both";
            string? trimChars = null;
            
            if (rule.ConversionParams?.TryGetValue("trim", out var trimParams) == true)
            {
                var trimElement = trimParams is JsonElement je ? je : JsonSerializer.SerializeToElement(trimParams);
                if (trimElement.ValueKind == JsonValueKind.String)
                {
                    // 简化格式：直接是字符串值，表示 type
                    trimType = trimElement.GetString()?.ToLowerInvariant() ?? "both";
                }
                else if (trimElement.ValueKind == JsonValueKind.Object)
                {
                    // 完整格式：嵌套对象
                    if (trimElement.TryGetProperty("type", out var typeProperty))
                    {
                        trimType = typeProperty.GetString()?.ToLowerInvariant() ?? "both";
                    }
                    if (trimElement.TryGetProperty("chars", out var charsProperty))
                    {
                        trimChars = charsProperty.GetString();
                    }
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