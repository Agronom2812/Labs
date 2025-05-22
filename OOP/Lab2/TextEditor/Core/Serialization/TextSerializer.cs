using TextEditor.Core.Documents;

namespace TextEditor.Core.Serialization;

public class TextSerializer : IDocumentSerializer {
    public string FileExtension => ".txt";

    public string? Serialize(Document? document)
    {
        return document.Content;
    }

    public Document? Deserialize(string data)
    {
        return new PlainTextDocument { Content = data };
    }
}
