using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ConfigLink.Converters
{
    public class JoinConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            var separator = rule.ConversionParams?["join"]?.ToString() ?? "";

            // 如果输入是数组
            if (value.ValueKind == JsonValueKind.Array)
            {
                var items = new List<string>();
                foreach (var item in value.EnumerateArray())
                {
                    items.Add(item.ToString() ?? "");
                }
                return string.Join(separator, items);
            }

            // 如果输入是对象，尝试转换为字符串数组后连接
            if (value.ValueKind == JsonValueKind.Object)
            {
                var items = new List<string>();
                foreach (var property in value.EnumerateObject())
                {
                    items.Add(property.Value.ToString() ?? "");
                }
                return string.Join(separator, items);
            }

            // 其他情况直接返回字符串表示
            return value.ToString();
        }
    }
}