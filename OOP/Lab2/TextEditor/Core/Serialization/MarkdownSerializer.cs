using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;

namespace TextEditor.Core.Serialization;

public sealed class MarkdownSerializer(INotificationService notificationService) : IDocumentSerializer {
    public string FileExtension => ".md";

    public string Serialize(Document? document) => document?.Content ?? string.Empty;

    public Document Deserialize(string data) {
        return new MarkdownDocument(notificationService) { Content = data };
    }
}
