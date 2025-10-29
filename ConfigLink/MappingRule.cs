using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ConfigLink
{
    public class MappingRule
    {
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("target")]
        public string Target { get; set; } = string.Empty;

        [JsonPropertyName("conversion")]
        public List<string>? Conversion { get; set; }

        [JsonPropertyName("conversion_params")]
        public Dictionary<string, object>? ConversionParams { get; set; }
    }
}
