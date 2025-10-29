using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConfigLink.Converters
{
    public class ReplaceConverter : IConverter
    {
        public object? Convert(JsonElement value, MappingRule rule, MappingEngine engine)
        {
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText().Trim('"');
            if (text == null)
                return null;

            // 尝试从 "replace" 键下获取参数
            string? search = null;
            string replace = "";
            bool useRegex = false;
            bool ignoreCase = false;
            
            if (rule.ConversionParams?.TryGetValue("replace", out var replaceParams) == true && replaceParams is JsonElement replaceElement)
            {
                if (replaceElement.TryGetProperty("from", out var fromProperty))
                {
                    search = fromProperty.GetString();
                }
                else if (replaceElement.TryGetProperty("search", out var searchProperty))
                {
                    search = searchProperty.GetString();
                }
                
                if (replaceElement.TryGetProperty("to", out var toProperty))
                {
                    replace = toProperty.GetString() ?? "";
                }
                else if (replaceElement.TryGetProperty("replace", out var replaceProperty))
                {
                    replace = replaceProperty.GetString() ?? "";
                }
                
                if (replaceElement.TryGetProperty("useRegex", out var useRegexProperty))
                {
                    useRegex = useRegexProperty.ValueKind == JsonValueKind.True || 
                              (useRegexProperty.ValueKind == JsonValueKind.String && 
                               useRegexProperty.GetString()?.ToLowerInvariant() == "true");
                }
                else if (replaceElement.TryGetProperty("regex", out var regexProperty))
                {
                    useRegex = regexProperty.ValueKind == JsonValueKind.True || 
                              (regexProperty.ValueKind == JsonValueKind.String && 
                               regexProperty.GetString()?.ToLowerInvariant() == "true");
                }
                if (replaceElement.TryGetProperty("ignoreCase", out var ignoreCaseProperty))
                {
                    ignoreCase = ignoreCaseProperty.GetString()?.ToLowerInvariant() == "true";
                }
            }

            if (string.IsNullOrEmpty(search))
                return text;

            if (useRegex)
            {
                var options = RegexOptions.None;
                if (ignoreCase)
                    options |= RegexOptions.IgnoreCase;

                try
                {
                    return Regex.Replace(text, search, replace, options);
                }
                catch (ArgumentException)
                {
                    // 如果正则表达式无效，回退到普通字符串替换
                    return ReplaceString(text, search, replace, ignoreCase);
                }
            }

            return ReplaceString(text, search, replace, ignoreCase);
        }

        private static string ReplaceString(string text, string search, string replace, bool ignoreCase)
        {
            if (ignoreCase)
            {
                var comparison = StringComparison.OrdinalIgnoreCase;
                var index = text.IndexOf(search, comparison);
                var result = text;

                while (index >= 0)
                {
                    result = result.Substring(0, index) + replace + result.Substring(index + search.Length);
                    index = result.IndexOf(search, index + replace.Length, comparison);
                }

                return result;
            }

            return text.Replace(search, replace);
        }
    }
}