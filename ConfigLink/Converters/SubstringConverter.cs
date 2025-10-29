using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigLink.Converters
{
    public class SubstringConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText().Trim('"');
            if (text == null)
                return null;

            // 尝试从 "substring" 键下获取参数
            string? startParam = null;
            string? lengthParam = null;
            string? endParam = null;
            
            if (rule.ConversionParams?.TryGetValue("substring", out var substringParams) == true && substringParams is JsonElement substringElement)
            {
                if (substringElement.TryGetProperty("start", out var startProperty))
                {
                    startParam = startProperty.ValueKind == JsonValueKind.Number ? 
                                startProperty.GetInt32().ToString() : 
                                startProperty.GetString();
                }
                if (substringElement.TryGetProperty("length", out var lengthProperty))
                {
                    lengthParam = lengthProperty.ValueKind == JsonValueKind.Number ? 
                                 lengthProperty.GetInt32().ToString() : 
                                 lengthProperty.GetString();
                }
                if (substringElement.TryGetProperty("end", out var endProperty))
                {
                    endParam = endProperty.ValueKind == JsonValueKind.Number ? 
                              endProperty.GetInt32().ToString() : 
                              endProperty.GetString();
                }
            }

            if (!int.TryParse(startParam, out var start))
                start = 0;

            // 处理负数索引（从末尾开始计算）
            if (start < 0)
                start = Math.Max(0, text.Length + start);

            // 确保 start 不超出字符串范围
            if (start >= text.Length)
                return "";

            // 如果指定了 end 参数
            if (!string.IsNullOrEmpty(endParam) && int.TryParse(endParam, out var end))
            {
                // 处理负数 end（从末尾开始计算）
                if (end < 0)
                    end = text.Length + end;

                // 确保 end 不小于 start
                if (end <= start)
                    return "";

                // 确保 end 不超出字符串范围
                end = Math.Min(end, text.Length);

                return text.Substring(start, end - start);
            }

            // 如果指定了 length 参数
            if (!string.IsNullOrEmpty(lengthParam) && int.TryParse(lengthParam, out var length))
            {
                if (length <= 0)
                    return "";

                // 确保不超出字符串范围
                length = Math.Min(length, text.Length - start);
                return text.Substring(start, length);
            }

            // 如果只指定了 start，返回从 start 到字符串末尾的部分
            return text.Substring(start);
        }
    }
}