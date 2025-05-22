using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;
using TextEditor.Core.Serialization;

namespace TextEditor.Core.Services;

public sealed class DocumentSerializerService
{
    private readonly Dictionary<string, IDocumentSerializer> _serializers;
    private readonly INotificationService _notificationService;

    public DocumentSerializerService(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _serializers = new Dictionary<string, IDocumentSerializer>
        {
            ["txt"] = new TextSerializer(_notificationService),
            ["json"] = new JsonDocumentSerializer(_notificationService),
            ["xml"] = new XmlSerializer(_notificationService),
            ["md"] = new MarkdownSerializer(_notificationService)
        };
    }

    public void Save(Document doc, string path, string format)
    {
        var serializer = GetSerializer(format);
        string finalPath = EnsureExtension(path, serializer.FileExtension);
        File.WriteAllText(finalPath, serializer.Serialize(doc));
    }

    public Document? Load(string path)
    {
        string ext = Path.GetExtension(path).TrimStart('.');
        if (!_serializers.TryGetValue(ext, out var serializer))
            throw new NotSupportedException($"File format '{ext}' not supported");

        return serializer.Deserialize(File.ReadAllText(path));
    }

    public static void DeleteDocument(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Document not found", path);

        File.Delete(path);
    }

    private IDocumentSerializer GetSerializer(string format)
    {
        return format.ToLower() switch
        {
            "txt" => new TextSerializer(_notificationService),
            "json" => new JsonDocumentSerializer(_notificationService),
            "xml" => new XmlSerializer(_notificationService),
            "md" => new MarkdownSerializer(_notificationService),
            _ => throw new NotSupportedException($"Формат {format} не поддерживается")
        };
    }

    private static string EnsureExtension(string path, string extension)
        => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? path
            : $"{path}{extension}";
}
