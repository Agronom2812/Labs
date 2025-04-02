using System.Text.Json;
using System.Text.Json.Serialization;
using ConsolePaint.Shapes;

namespace ConsolePaint.Utilities;

public sealed class ShapeConverter : JsonConverter<IShape>
{
    public override IShape? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        string? type = root.GetProperty("Type").GetString();

        return type switch
        {
            "Circle" => root.Deserialize<Circle>(options),
            "Rectangle" => root.Deserialize<Rectangle>(options),
            "Triangle" => root.Deserialize<Triangle>(options),
            "Line" => root.Deserialize<Line>(options),
            _ => throw new JsonException($"Unknown shape type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, IShape value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", value.GetType().Name);

        switch (value)
        {
            case Circle circle:
                JsonSerializer.Serialize(writer, circle, options);
                break;
            case Rectangle rect:
                JsonSerializer.Serialize(writer, rect, options);
                break;
            case Triangle triangle:
                JsonSerializer.Serialize(writer, triangle, options);
                break;
            case Line line:
                JsonSerializer.Serialize(writer, line, options);
                break;
        }

        writer.WriteEndObject();
    }
}
