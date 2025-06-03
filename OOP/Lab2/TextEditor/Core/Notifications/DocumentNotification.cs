using TextEditor.Core.Documents;
using TextEditor.Core.Users;

namespace TextEditor.Core.Notifications;

public sealed class DocumentNotification(User user, string message) {
    public User User { get; } = user;
    public string Message { get; } = message;
    public Document? Document { get; }
    public DateTime Timestamp { get; } = DateTime.Now;

    public DocumentNotification(Document? document, User user, string message) : this(user, message) {
        Document = document;
        User = user;
        Message = message;
        Timestamp = DateTime.Now;
    }
}
