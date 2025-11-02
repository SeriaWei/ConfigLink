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
        public object? Convert(JsonElement value, JsonElement conversionParams, MappingEngine engine)
        {
            string outputType = "decimal";
            string? format = null;
            string culture = "invariant";
            
            if (conversionParams.ValueKind == JsonValueKind.String)
            {
                outputType = conversionParams.GetString()?.ToLowerInvariant() ?? "decimal";
            }
            else if (conversionParams.ValueKind == JsonValueKind.Object)
            {
                if (conversionParams.TryGetProperty("type", out var typeProperty))
                {
                    outputType = typeProperty.GetString()?.ToLowerInvariant() ?? "decimal";
                }
                if (conversionParams.TryGetProperty("format", out var formatProperty))
                {
                    format = formatProperty.GetString();
                }
                if (conversionParams.TryGetProperty("culture", out var cultureProperty))
                {
                    culture = cultureProperty.GetString() ?? "invariant";
                }
            }

            var cultureInfo = culture.ToLowerInvariant() switch
            {
                "invariant" => CultureInfo.InvariantCulture,
                "current" => CultureInfo.CurrentCulture,
                _ => CultureInfo.GetCultureInfo(culture)
            };

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

            object result = outputType switch
            {
                "int" or "integer" => (int)numericValue,
                "long" => (long)numericValue,
                "float" => (float)numericValue,
                "double" => (double)numericValue,
                "decimal" => numericValue,
                _ => numericValue
            };

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