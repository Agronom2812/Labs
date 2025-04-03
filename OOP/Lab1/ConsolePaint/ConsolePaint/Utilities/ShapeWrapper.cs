using System.Text.Json;
using System.Text.Json.Serialization;
using ConsolePaint.Shapes;

namespace ConsolePaint.Utilities;

public sealed class ShapeWrapper(IShape shape) {

    [JsonPropertyName("Type")] private string Type { get; set; } = shape.GetType().Name;

    [JsonPropertyName("Data")] private JsonElement Data { get; set; } = JsonSerializer.SerializeToElement(shape, shape.GetType(), s_options);

    private static readonly JsonSerializerOptions? s_options = new();


    public IShape? GetShape()
    {
        return Type switch
        {
            "Line" => JsonSerializer.Deserialize<Line>(Data.GetRawText(), s_options),
            "Rectangle" => JsonSerializer.Deserialize<Rectangle>(Data.GetRawText(), s_options),
            "Circle" => JsonSerializer.Deserialize<Circle>(Data.GetRawText(), s_options),
            "Triangle" => JsonSerializer.Deserialize<Triangle>(Data.GetRawText(), s_options),
            _ => throw new NotSupportedException($"Type {Type} is not supported")
        };
    }
}
