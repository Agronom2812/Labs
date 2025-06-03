using TextEditor.Core.Documents;

namespace TextEditor.Core.Notifications;

public interface INotificationService {
    void Notify(Document? document, string message);
}
