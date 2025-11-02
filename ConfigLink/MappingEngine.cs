using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigLink.Converters;
using System.Text.Json;
using ConfigLink.Extensions;

namespace ConfigLink
{
    public class MappingEngine
    {
        private readonly List<MappingRule> _rules;
        private readonly Dictionary<string, IConverter> _converters;

        public MappingEngine(List<MappingRule> rules)
        {
            _rules = rules;

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

        public void RegisterConverter(string name, IConverter converter)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("转换器名称不能为空", nameof(name));
            
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            _converters[name] = converter;
        }

        public Dictionary<string, object?> Transform(object sourceObj)
        {
            JsonElement root=JsonSerializer.SerializeToElement(sourceObj);
            return Process(root, _rules);
        }

        internal Dictionary<string, object?> Process(JsonElement element, List<MappingRule> rules)
        {
            var result = new Dictionary<string, object?>();

            foreach (var rule in rules)
            {
                var value = GetValueByPath(element, rule.Source);

                if (value == null) continue; 

                var converted = ApplyConversions(value.Value, rule, this);
                var target = rule.Target;

                if (target == "$root")
                {
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
            return root.GetByPath(path);
        }

        private object? ApplyConversions(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            object? cur = value.Clone();

            if (rule.Conversion == null) return ConvertToPrimitive(cur);

            foreach (var op in rule.Conversion)
            {
                if (!_converters.TryGetValue(op, out var converter))
                    continue;

                JsonElement currentElement = cur switch
                {
                    JsonElement je => je,
                    _ => JsonSerializer.SerializeToElement(cur)
                };

                JsonElement conversionParams = default;
                if (rule.ConversionParams?.TryGetValue(op, out var paramObj) == true)
                {
                    conversionParams = paramObj is JsonElement je ? je : JsonSerializer.SerializeToElement(paramObj);
                }

                cur = converter.Convert(currentElement, conversionParams, engine);
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
