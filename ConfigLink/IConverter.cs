using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigLink
{
    public interface IConverter
    {
        object? Convert(JsonElement value, MappingRule rule, MappingEngine engine);
    }
}
