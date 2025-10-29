using ConfigLink;
using System.Text.Json;

namespace Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var engine = new MappingEngine(File.ReadAllText("config/mapping.json"));

            var sourceJson = File.ReadAllText("config/sample_input.json");

            var result = engine.Transform(sourceJson);

            var options = new JsonSerializerOptions { WriteIndented = true };
            Console.WriteLine(JsonSerializer.Serialize(result, options));
        }
    }
}