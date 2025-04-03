using System.Text.Json;
using System.Text.Json.Serialization;
using ConsolePaint.Shapes;

namespace ConsolePaint.Utilities
{
    public static class CanvasSerializer
    {
        private static readonly JsonSerializerOptions? s_options = new()
        {
            WriteIndented = true,
            Converters = { new ShapeConverter() }
        };

        public static string Serialize(IEnumerable<IShape> shapes)
        {
            var wrapperList = shapes.Select(shape => new ShapeWrapper(shape)).ToList();
            return JsonSerializer.Serialize(wrapperList, s_options);
        }

        public static List<IShape?> Deserialize(string json)
        {
            var wrapperList = JsonSerializer.Deserialize<List<ShapeWrapper>>(json, s_options);
            return wrapperList?.Select(w => w.GetShape()).ToList() ?? [];
        }
    }
}
