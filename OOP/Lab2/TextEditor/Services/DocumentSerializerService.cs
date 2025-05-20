using TextEditor.Core.Documents;
using TextEditor.Core.Serialization;

namespace TextEditor.Services;

public class DocumentSerializerService
{
    private readonly Dictionary<string, IDocumentSerializer> _serializers = new()
    {
        ["txt"] = new TextSerializer(),
        ["json"] = new JsonDocumentSerializer(),
        ["xml"] = new XmlSerializer()
    };

    public void Save(Document doc, string? path, string format)
    {
        if (!_serializers.TryGetValue(format, out var serializer))
            throw new NotSupportedException($"Format '{format}' not supported");

        File.WriteAllText(AddExtension(path, serializer.FileExtension),
            serializer.Serialize(doc));
    }

    public Document Load(string? path)
    {
        string ext = Path.GetExtension(path).TrimStart('.');
        if (!_serializers.TryGetValue(ext, out var serializer))
            throw new NotSupportedException($"File format '{ext}' not supported");

        return serializer.Deserialize(File.ReadAllText(path));
    }

    private static string? AddExtension(string? path, string ext)
        => path.EndsWith(ext) ? path : $"{path}{ext}";
}
