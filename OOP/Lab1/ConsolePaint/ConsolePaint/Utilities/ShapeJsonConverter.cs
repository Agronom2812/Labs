using ConsolePaint.Shapes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsolePaint.Utilities;

public sealed class ShapeJsonConverter : JsonConverter<IShape>
{

    public override IShape? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("Type", out var typeProp))
            throw new JsonException("Missing shape type discriminator");

        return typeProp.GetString() switch
        {
            nameof(Triangle) => JsonSerializer.Deserialize<Triangle>(root.GetRawText(), options),
            nameof(Rectangle) => JsonSerializer.Deserialize<Rectangle>(root.GetRawText(), options),
            nameof(Circle) => JsonSerializer.Deserialize<Circle>(root.GetRawText(), options),
            _ => throw new JsonException($"Unknown shape type: {typeProp.GetString()}")
        };
    }

    public override void Write(Utf8JsonWriter writer, IShape value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", value.GetType().Name);

        foreach (var prop in value.GetType().GetProperties()) {
            if (!prop.CanRead) continue;

            object? propValue = prop.GetValue(value);
            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, propValue, options);
        }

        writer.WriteEndObject();
    }
}
