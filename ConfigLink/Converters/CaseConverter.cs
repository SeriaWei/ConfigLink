using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConfigLink.Converters
{
    public class CaseConverter : IConverter
    {
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText().Trim('"');
            if (string.IsNullOrEmpty(text))
                return text;

            string caseType = "lower";
            if (conversionParams.ValueKind == JsonValueKind.String)
            {
                caseType = conversionParams.GetString()?.ToLowerInvariant() ?? "lower";
            }
            else if (conversionParams.ValueKind == JsonValueKind.Object && conversionParams.TryGetProperty("case", out var caseProperty))
            {
                caseType = caseProperty.GetString()?.ToLowerInvariant() ?? "lower";
            }

            return caseType switch
            {
                "upper" or "uppercase" => text.ToUpperInvariant(),
                "lower" or "lowercase" => text.ToLowerInvariant(),
                "title" or "titlecase" => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLowerInvariant()),
                "camel" or "camelcase" => ToCamelCase(text),
                "pascal" or "pascalcase" => ToPascalCase(text),
                "kebab" or "kebabcase" => ToKebabCase(text),
                "snake" or "snakecase" => ToSnakeCase(text),
                _ => text
            };
        }

        private static string ToCamelCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var words = Regex.Split(text, @"[\s_-]+", RegexOptions.IgnoreCase)
                            .Where(w => !string.IsNullOrEmpty(w))
                            .ToArray();

            if (words.Length == 0)
                return text;

            var result = new StringBuilder();
            result.Append(words[0].ToLowerInvariant());

            for (int i = 1; i < words.Length; i++)
            {
                result.Append(char.ToUpperInvariant(words[i][0]));
                if (words[i].Length > 1)
                    result.Append(words[i].Substring(1).ToLowerInvariant());
            }

            return result.ToString();
        }

        private static string ToPascalCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var words = Regex.Split(text, @"[\s_-]+", RegexOptions.IgnoreCase)
                            .Where(w => !string.IsNullOrEmpty(w))
                            .ToArray();

            var result = new StringBuilder();
            foreach (var word in words)
            {
                result.Append(char.ToUpperInvariant(word[0]));
                if (word.Length > 1)
                    result.Append(word.Substring(1).ToLowerInvariant());
            }

            return result.ToString();
        }

        private static string ToKebabCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = Regex.Replace(text, @"([a-z])([A-Z])", "$1-$2");
            
            text = Regex.Replace(text, @"[\s_]+", "-");
            
            return text.ToLowerInvariant();
        }

        private static string ToSnakeCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = Regex.Replace(text, @"([a-z])([A-Z])", "$1_$2");
            
            text = Regex.Replace(text, @"[\s-]+", "_");
            
            return text.ToLowerInvariant();
        }
    }
}