using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigLink.Converters;
using System.Text.Json;

namespace ConfigLink
{
    public class MappingEngine
    {
        private readonly List<MappingRule> _rules;
        private readonly Dictionary<string, IConverter> _converters;

        public MappingEngine(string json)
        {
            var doc = JsonDocument.Parse(json);
            _rules = doc.RootElement
                        .GetProperty("mappings")
                        .Deserialize<List<MappingRule>>()!;

            // 注册所有 converter
            _converters = new()
            {
                ["format"] = new FormatConverter(),
                ["prepend"] = new PrependConverter(),
                ["map_array"] = new MapArrayConverter(),
                ["map_object"] = new MapObjectConverter(),
                ["to_array"] = new ToArrayConverter(),
                ["join"] = new JoinConverter(),
                ["case"] = new CaseConverter(),
                ["trim"] = new TrimConverter(),
                ["replace"] = new ReplaceConverter(),
                ["substring"] = new SubstringConverter(),
                ["default"] = new DefaultConverter(),
                ["number"] = new NumberConverter(),
                ["boolean"] = new BooleanConverter()
            };
        }

        /// <summary>
        /// 入口：把 sourceJson 转换成目标字典
        /// </summary>
        public Dictionary<string, object?> Transform(string sourceJson)
        {
            var root = JsonDocument.Parse(sourceJson).RootElement;
            return Process(root, _rules);
        }

        internal Dictionary<string, object?> Process(JsonElement element, List<MappingRule> rules)
        {
            var result = new Dictionary<string, object?>();

            foreach (var rule in rules)
            {
                var value = GetValueByPath(element, rule.Source);

                if (value == null) continue; // 路径不存在直接跳过

                var converted = ApplyConversions(value.Value, rule, this);
                var target = rule.Target;

                if (target == "$root")
                {
                    // 展开到根
                    if (converted is Dictionary<string, object?> dict)
                    {
                        foreach (var kv in dict)
                            result[kv.Key] = kv.Value;
                    }
                }
                else
                {
                    result[target] = converted;
                }
            }
            return result;
        }

        internal JsonElement? GetValueByPath(JsonElement root, string path)
        {
            var parts = path.Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries);
            JsonElement current = root;

            foreach (var part in parts)
            {
                var cleaned = part.TrimEnd(']');
                if (cleaned.All(char.IsDigit)) // 数组索引
                {
                    var idx = int.Parse(cleaned);
                    if (current.ValueKind != JsonValueKind.Array || idx >= current.GetArrayLength())
                        return null;
                    current = current[idx];
                }
                else // 对象属性
                {
                    if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(cleaned, out current))
                        return null;
                }
            }
            return current;
        }

        private object? ApplyConversions(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            object? cur = value.Clone(); // 保持原始 JsonElement

            if (rule.Conversion == null) return ConvertToPrimitive(cur);

            foreach (var op in rule.Conversion)
            {
                if (!_converters.TryGetValue(op, out var converter))
                    continue; // 未知操作直接跳过

                // 需要将当前值转换为 JsonElement 传递给转换器
                JsonElement currentElement = cur switch
                {
                    JsonElement je => je,
                    _ => JsonSerializer.SerializeToElement(cur)
                };

                cur = converter.Convert(currentElement, rule, engine);
                if (cur == null) break;
            }
            return cur;
        }

        private static object? ConvertToPrimitive(object? obj)
        {
            return obj switch
            {
                JsonElement je => je.ValueKind switch
                {
                    JsonValueKind.String => je.GetString(),
                    JsonValueKind.Number => je.TryGetInt64(out var i) ? i : je.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => je.GetRawText()
                },
                _ => obj
            };
        }
    }
}
