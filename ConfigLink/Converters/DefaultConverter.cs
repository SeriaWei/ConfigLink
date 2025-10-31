using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigLink.Converters
{
    public class DefaultConverter : IConverter
    {
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            // 从传入的 conversionParams 中获取参数
            object? defaultValueObj = null;
            string condition = "null";

            if (conversionParams.ValueKind != JsonValueKind.Undefined)
            {
                if (conversionParams.TryGetProperty("value", out var valueProperty))
                {
                    if (valueProperty.ValueKind == JsonValueKind.String)
                        defaultValueObj = valueProperty.GetString();
                    else
                        defaultValueObj = valueProperty;
                }
                if (conversionParams.TryGetProperty("condition", out var conditionProperty))
                {
                    condition = conditionProperty.GetString()?.ToLowerInvariant() ?? "null";
                }
            }

            // 检查是否应该使用默认值
            bool useDefault = condition switch
            {
                "null" => value.ValueKind == JsonValueKind.Null,
                "empty" => IsEmpty(value),
                "nullorempty" => value.ValueKind == JsonValueKind.Null || IsEmpty(value),
                "whitespace" => IsWhitespace(value),
                "nullorwhitespace" => value.ValueKind == JsonValueKind.Null || IsWhitespace(value),
                _ => value.ValueKind == JsonValueKind.Null
            };

            if (useDefault)
            {
                return defaultValueObj;
            }

            // 返回原始值
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => value.GetRawText()
            };
        }

        private static bool IsEmpty(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => string.IsNullOrEmpty(value.GetString()),
                JsonValueKind.Array => value.GetArrayLength() == 0,
                JsonValueKind.Object => value.EnumerateObject().Count() == 0,
                _ => false
            };
        }

        private static bool IsWhitespace(JsonElement value)
        {
            return value.ValueKind == JsonValueKind.String &&
                   string.IsNullOrWhiteSpace(value.GetString());
        }
    }
}