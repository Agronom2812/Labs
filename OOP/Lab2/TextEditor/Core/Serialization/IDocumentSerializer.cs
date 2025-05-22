using TextEditor.Core.Documents;

namespace TextEditor.Core.Serialization;

public interface IDocumentSerializer {
    string? Serialize(Document? document);
    Document? Deserialize(string data);
    string FileExtension { get; }
}
