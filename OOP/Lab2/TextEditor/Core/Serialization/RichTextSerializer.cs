using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;

namespace TextEditor.Core.Serialization;

public sealed class RichTextSerializer(INotificationService notificationService) : IDocumentSerializer {
    public string Serialize(Document? document) {
        return document?.Content ?? string.Empty;
    }

    public Document Deserialize(string data) {
        return new RichTextDocument(notificationService) { Content = data };
    }
}
