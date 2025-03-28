using System.Text.Json;
using System.Text.Json.Serialization;
using ConsolePaint.Shapes;

namespace ConsolePaint.Utilities
{
    public static class CanvasSerializer
    {
        private static readonly JsonSerializerOptions? options = new()
        {
            WriteIndented = true,
            Converters = { new ShapeConverter() }
        };

        public static string Serialize(List<IShape> shapes)
        {
            var wrapperList = shapes.Select(shape => new ShapeWrapper(shape)).ToList();
            return JsonSerializer.Serialize(wrapperList, options);
        }

        public static List<IShape> Deserialize(string json)
        {
            var wrapperList = JsonSerializer.Deserialize<List<ShapeWrapper>>(json, options);
            return wrapperList?.Select(w => w.GetShape()).ToList() ?? new List<IShape>();
        }
    }
}
