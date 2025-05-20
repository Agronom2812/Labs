using TextEditor.Core.Serialization;

namespace TextEditor.Core.Factories;

public class SerializerFactory {
    public static IDocumentSerializer GetSerializer(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".txt" => new TextSerializer(),
            ".json" => new JsonDocumentSerializer(),
            ".xml" => new XmlSerializer(),
            _ => throw new NotSupportedException("Unsupported file format")
        };
    }
}
