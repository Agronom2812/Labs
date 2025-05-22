using TextEditor.Core.Documents;
using TextEditor.Core.Serialization;

namespace TextEditor.Services;

public sealed class DocumentSerializerService
{
    private readonly Dictionary<string, IDocumentSerializer> _serializers = new()
    {
        ["txt"] = new TextSerializer(),
        ["json"] = new JsonDocumentSerializer(),
        ["xml"] = new XmlSerializer()
    };

    public static void Save(Document? doc, string path, string format)
    {
        var serializer = GetSerializer(format);
        string finalPath = EnsureExtension(path, serializer.FileExtension);
        File.WriteAllText(finalPath, serializer.Serialize(doc));
    }

    public Document? Load(string? path)
    {
        string? ext = Path.GetExtension(path)?.TrimStart('.');
        if (!_serializers.TryGetValue(ext, out var serializer))
            throw new NotSupportedException($"File format '{ext}' not supported");

        return serializer.Deserialize(File.ReadAllText(path ?? throw new ArgumentNullException(nameof(path))));
    }

    public static void DeleteDocument(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Document not found", path);

        File.Delete(path);
    }

    private static IDocumentSerializer GetSerializer(string format)
    {
        return format.ToLower() switch
        {
            "txt" => new TextSerializer(),
            "json" => new JsonDocumentSerializer(),
            "xml" => new XmlSerializer(),
            _ => throw new NotSupportedException($"Формат {format} не поддерживается")
        };
    }

    private static string EnsureExtension(string path, string extension)
        => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? path
            : $"{path}{extension}";
}
