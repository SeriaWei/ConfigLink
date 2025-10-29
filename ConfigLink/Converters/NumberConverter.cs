using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigLink.Converters
{
    public class NumberConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            // 支持两种参数格式：
            // 1. 简化格式：{"number": "int"} - 只指定 type
            // 2. 完整格式：{"number": {"type": "int", "format": "N2"}}
            string outputType = "decimal";
            string? format = null;
            string culture = "invariant";
            
            if (rule.ConversionParams?.TryGetValue("number", out var numberParams) == true && numberParams is JsonElement numberElement)
            {
                if (numberElement.ValueKind == JsonValueKind.String)
                {
                    // 简化格式：直接是字符串值，表示 type
                    outputType = numberElement.GetString()?.ToLowerInvariant() ?? "decimal";
                }
                else if (numberElement.ValueKind == JsonValueKind.Object)
                {
                    // 完整格式：嵌套对象
                    if (numberElement.TryGetProperty("type", out var typeProperty))
                    {
                        outputType = typeProperty.GetString()?.ToLowerInvariant() ?? "decimal";
                    }
                    if (numberElement.TryGetProperty("format", out var formatProperty))
                    {
                        format = formatProperty.GetString();
                    }
                    if (numberElement.TryGetProperty("culture", out var cultureProperty))
                    {
                        culture = cultureProperty.GetString() ?? "invariant";
                    }
                }
            }

            var cultureInfo = culture.ToLowerInvariant() switch
            {
                "invariant" => CultureInfo.InvariantCulture,
                "current" => CultureInfo.CurrentCulture,
                _ => CultureInfo.GetCultureInfo(culture)
            };

            // 首先尝试获取数值
            decimal numericValue = 0;
            bool hasValue = false;

            switch (value.ValueKind)
            {
                case JsonValueKind.Number:
                    numericValue = value.GetDecimal();
                    hasValue = true;
                    break;
                case JsonValueKind.String:
                    var stringValue = value.GetString();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        hasValue = decimal.TryParse(stringValue, NumberStyles.Any, cultureInfo, out numericValue);
                    }
                    break;
            }

            if (!hasValue)
                return null;

            // 根据输出类型转换
            object result = outputType switch
            {
                "int" or "integer" => (int)numericValue,
                "long" => (long)numericValue,
                "float" => (float)numericValue,
                "double" => (double)numericValue,
                "decimal" => numericValue,
                _ => numericValue
            };

            // 如果指定了格式，返回格式化字符串
            if (!string.IsNullOrEmpty(format))
            {
                return result switch
                {
                    int i => i.ToString(format, cultureInfo),
                    long l => l.ToString(format, cultureInfo),
                    float fl => fl.ToString(format, cultureInfo),
                    double d => d.ToString(format, cultureInfo),
                    decimal dec => dec.ToString(format, cultureInfo),
                    _ => result.ToString()
                };
            }

            return result;
        }
    }
}