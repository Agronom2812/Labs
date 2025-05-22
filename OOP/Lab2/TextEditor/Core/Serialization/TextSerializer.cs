using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;

namespace TextEditor.Core.Serialization;

public sealed class TextSerializer : IDocumentSerializer
{
    private readonly INotificationService _notificationService;

    public TextSerializer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public string FileExtension => ".txt";

    public string? Serialize(Document? document) => document?.Content;

    public Document? Deserialize(string data)
    {
        return new PlainTextDocument(_notificationService) { Content = data };
    }
}
