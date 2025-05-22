using TextEditor.Core.Users;

namespace TextEditor.Core.Notifications;

public class DocumentNotification(User user, string message) {
    public User User { get; } = user;
    public string Message { get; } = message;
    public DateTime Timestamp { get; } = DateTime.Now;
}
