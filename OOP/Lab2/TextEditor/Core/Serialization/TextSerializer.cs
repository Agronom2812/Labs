using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;

namespace TextEditor.Core.Serialization;

public sealed class TextSerializer(INotificationService notificationService) : IDocumentSerializer {
    public string FileExtension => ".txt";

    public string? Serialize(Document? document) => document?.Content;

    public Document Deserialize(string data) {
        return new PlainTextDocument(notificationService) { Content = data };
    }
}
