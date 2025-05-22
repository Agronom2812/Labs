using System.Text.Json;
using TextEditor.Core.Documents;

namespace TextEditor.Core.Serialization;

public sealed class JsonDocumentSerializer : IDocumentSerializer{
    public string FileExtension => ".json";

    public string Serialize(Document? document)
    {
        return JsonSerializer.Serialize(new {
            document.Title,
            document.Content,
            Type = document.GetType().Name
        });
    }

    public Document? Deserialize(string data)
    {
        var json = JsonDocument.Parse(data).RootElement;
        return json.GetProperty("Type").GetString() switch
        {
            nameof(PlainTextDocument) => new PlainTextDocument {
                Title = json.GetProperty("Title").GetString(),
                Content = json.GetProperty("Content").GetString()
            },
            _ => throw new NotSupportedException("Document type not supported")
        };
    }
}
