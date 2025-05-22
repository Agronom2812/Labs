using TextEditor.Core.Notifications;
using TextEditor.Core.Serialization;

namespace TextEditor.Core.Factories;

public static class SerializerFactory
{
    public static IDocumentSerializer GetSerializer(string filePath, INotificationService notificationService)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".txt" => new TextSerializer(notificationService),
            ".json" => new JsonDocumentSerializer(notificationService),
            ".xml" => new XmlSerializer(notificationService),
            ".md" => new MarkdownSerializer(notificationService),
            _ => throw new NotSupportedException("Unsupported file format")
        };
    }
}
