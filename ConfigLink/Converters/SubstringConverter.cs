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
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText().Trim('"');
            if (text == null)
                return null;

            string? startParam = null;
            string? lengthParam = null;
            string? endParam = null;

            if (conversionParams.ValueKind != JsonValueKind.Undefined)
            {
                if (conversionParams.TryGetProperty("start", out var startProperty))
                {
                    startParam = startProperty.ValueKind == JsonValueKind.Number ?
                                startProperty.GetInt32().ToString() :
                                startProperty.GetString();
                }
                if (conversionParams.TryGetProperty("length", out var lengthProperty))
                {
                    lengthParam = lengthProperty.ValueKind == JsonValueKind.Number ?
                                 lengthProperty.GetInt32().ToString() :
                                 lengthProperty.GetString();
                }
                if (conversionParams.TryGetProperty("end", out var endProperty))
                {
                    endParam = endProperty.ValueKind == JsonValueKind.Number ?
                              endProperty.GetInt32().ToString() :
                              endProperty.GetString();
                }
            }

            if (!int.TryParse(startParam, out var start))
                start = 0;

            if (start < 0)
                start = Math.Max(0, text.Length + start);

            if (start >= text.Length)
                return "";

            if (!string.IsNullOrEmpty(endParam) && int.TryParse(endParam, out var end))
            {
                if (end < 0)
                    end = text.Length + end;

                if (end <= start)
                    return "";

                end = Math.Min(end, text.Length);

                return text.Substring(start, end - start);
            }

            if (!string.IsNullOrEmpty(lengthParam) && int.TryParse(lengthParam, out var length))
            {
                if (length <= 0)
                    return "";

                length = Math.Min(length, text.Length - start);
                return text.Substring(start, length);
            }

            return text.Substring(start);
        }
    }
}