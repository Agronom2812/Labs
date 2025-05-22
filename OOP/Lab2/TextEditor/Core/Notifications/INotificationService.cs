using TextEditor.Core.Documents;
using TextEditor.Core.Users;

namespace TextEditor.Core.Notifications;

public interface INotificationService
{
    void Subscribe(User user, Document document);
    void Unsubscribe(User user, Document document);
    void Notify(Document document, string message);
}
