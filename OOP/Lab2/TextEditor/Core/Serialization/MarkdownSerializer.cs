using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;

namespace TextEditor.Core.Serialization;

public sealed class MarkdownSerializer : IDocumentSerializer
{
    private readonly INotificationService _notificationService;

    public MarkdownSerializer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public string FileExtension => ".md";

    public string Serialize(Document? document) => document?.Content ?? string.Empty;

    public Document? Deserialize(string data)
    {
        return new MarkdownDocument(_notificationService) { Content = data };
    }
}
