using TextEditor.Core.Notifications;
using TextEditor.Core.Serialization;

namespace TextEditor.Core.Factories;

public static class SerializerFactory {
    public static IDocumentSerializer GetSerializer(string filePath, INotificationService notificationService) {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty");

        string extension = GetExtension(filePath);

        return extension switch {
            ".txt" => new TextSerializer(notificationService),
            ".md" => new MarkdownSerializer(notificationService),
            ".rtf" => new RichTextSerializer(notificationService),
            ".json" => new JsonDocumentSerializer(notificationService),
            ".xml" => new XmlSerializer(notificationService),
            _ => throw new NotSupportedException($"Format '{extension}' is not supported. Supported formats:" +
                                                 $" .txt, .md, .rtf, .json, .xml")
        };
    }

    private static string GetExtension(string filePath) {
        if (!filePath.StartsWith("s3://")) return Path.GetExtension(filePath).ToLower();

        int lastDot = filePath.LastIndexOf('.');
        return lastDot > 0 ? filePath[lastDot..].ToLower() : "";
    }
}
