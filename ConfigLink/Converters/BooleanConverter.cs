using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigLink.Converters
{
    public class BooleanConverter : IConverter
    {
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            // 从传入的 conversionParams 中获取参数
            // 支持两种参数格式：
            // 1. 简化格式：直接是字符串值 "yesno" - 只指定 output 格式
            // 2. 完整格式：对象 {"output": "yesno", "trueValues": "yes,1"}
            string? trueValuesParam = null;
            string? falseValuesParam = null;
            string outputFormat = "boolean";
            
            if (conversionParams.ValueKind == JsonValueKind.String)
            {
                // 简化格式：直接是字符串值，表示 output 格式
                outputFormat = conversionParams.GetString()?.ToLowerInvariant() ?? "boolean";
            }
            else if (conversionParams.ValueKind == JsonValueKind.Object)
            {
                // 完整格式：嵌套对象
                if (conversionParams.TryGetProperty("trueValues", out var trueValuesProperty))
                {
                    trueValuesParam = trueValuesProperty.GetString();
                }
                if (conversionParams.TryGetProperty("falseValues", out var falseValuesProperty))
                {
                    falseValuesParam = falseValuesProperty.GetString();
                }
                if (conversionParams.TryGetProperty("output", out var outputProperty))
                {
                    outputFormat = outputProperty.GetString()?.ToLowerInvariant() ?? "boolean";
                }
            }

            var trueValues = !string.IsNullOrEmpty(trueValuesParam) 
                           ? trueValuesParam.Split(',', StringSplitOptions.RemoveEmptyEntries) 
                           : new[] { "true", "1", "yes", "on", "enabled" };
            
            var falseValues = !string.IsNullOrEmpty(falseValuesParam) 
                            ? falseValuesParam.Split(',', StringSplitOptions.RemoveEmptyEntries) 
                            : new[] { "false", "0", "no", "off", "disabled" };

            bool boolValue = false;
            bool hasValue = false;

            switch (value.ValueKind)
            {
                case JsonValueKind.True:
                    boolValue = true;
                    hasValue = true;
                    break;
                case JsonValueKind.False:
                    boolValue = false;
                    hasValue = true;
                    break;
                case JsonValueKind.Number:
                    var number = value.GetDecimal();
                    boolValue = number != 0;
                    hasValue = true;
                    break;
                case JsonValueKind.String:
                    var stringValue = value.GetString()?.Trim().ToLowerInvariant();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        if (trueValues.Any(tv => tv.ToLowerInvariant() == stringValue))
                        {
                            boolValue = true;
                            hasValue = true;
                        }
                        else if (falseValues.Any(fv => fv.ToLowerInvariant() == stringValue))
                        {
                            boolValue = false;
                            hasValue = true;
                        }
                    }
                    break;
            }

            if (!hasValue)
                return null;

            return outputFormat switch
            {
                "boolean" or "bool" => boolValue,
                "string" => boolValue.ToString().ToLowerInvariant(),
                "number" or "int" => boolValue ? 1 : 0,
                "yesno" => boolValue ? "yes" : "no",
                "onoff" => boolValue ? "on" : "off",
                "enableddisabled" => boolValue ? "enabled" : "disabled",
                _ => boolValue
            };
        }
    }
}