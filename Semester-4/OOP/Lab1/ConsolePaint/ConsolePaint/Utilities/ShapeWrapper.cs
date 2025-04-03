using System.Text.Json;
using System.Text.Json.Serialization;
using ConsolePaint.Shapes;

namespace ConsolePaint.Utilities;

public class ShapeWrapper
{

    [JsonPropertyName("Type")]
    public string Type { get; set; }

    [JsonPropertyName("Data")]
    public JsonElement Data { get; set; }

    private static readonly JsonSerializerOptions? options = new();


    public ShapeWrapper(IShape shape)
    {
        Type = shape.GetType().Name;
        Data = JsonSerializer.SerializeToElement(shape, shape.GetType(), options);
    }

    public IShape? GetShape()
    {
        return Type switch
        {
            "Line" => JsonSerializer.Deserialize<Line>(Data.GetRawText(), options),
            "Rectangle" => JsonSerializer.Deserialize<Rectangle>(Data.GetRawText(), options),
            "Circle" => JsonSerializer.Deserialize<Circle>(Data.GetRawText(), options),
            "Triangle" => JsonSerializer.Deserialize<Triangle>(Data.GetRawText(), options),
            _ => throw new NotSupportedException($"Type {Type} is not supported")
        };
    }
}
