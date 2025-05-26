using System.Text.Json;
using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;
using TextEditor.Core.Users;

namespace TextEditor.Core.Services;

public sealed class NotificationService : INotificationService {
    private readonly Dictionary<Document, List<User>> _documentSubscriptions;
    private List<DocumentNotification> _notificationHistory = [];
    private const string NotificationsFile = "notifications.json";
    private readonly UserManager _userManager;

    public NotificationService(UserManager userManager)
    {
        _userManager = userManager;
        _documentSubscriptions = new Dictionary<Document, List<User>>();
        _notificationHistory = [];
    }

    public bool IsSubscribed(User user, Document? document) {
        return _documentSubscriptions.ContainsKey(document) && _documentSubscriptions[document].Contains(user);
    }

    public List<DocumentNotification> GetUserNotifications(User user)
    {
        return _notificationHistory
            .Where(n => n.User == user || _documentSubscriptions
                .Where(kv => kv.Value.Contains(user))
                .Select(kv => kv.Key)
                .Contains(n.Document))
            .OrderByDescending(n => n.Timestamp)
            .ToList();
    }

    public void Subscribe(User user, Document? document) {
        if (!_documentSubscriptions.TryGetValue(document, out List<User>? value)) {
            value = [];
            _documentSubscriptions[document] = value;
        }

        if (value.Contains(user)) return;

        value.Add(user);
        SaveNotificationHistory();
    }

    public void Unsubscribe(User user, Document? document) {
        if (!_documentSubscriptions.TryGetValue(document, out List<User>? subscription)) return;

        subscription.Remove(user);
        SaveNotificationHistory();
    }

    public void Notify(Document document, string message)
    {
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message cannot be null or empty");

        if (!_documentSubscriptions.TryGetValue(document, out List<User>? value)) return;

        if (_userManager.CurrentUser == null)
            throw new InvalidOperationException("User must be logged in to send notifications");

        var notification = new DocumentNotification(
            document,
            _userManager.CurrentUser,
            message);

        _notificationHistory.Add(notification);
        SaveNotificationHistory();

        foreach (var user in value.Where(user => user != _userManager.CurrentUser)) {
            SendNotification(user, notification);
        }
    }

    private static void SendNotification(User user, DocumentNotification notification) {
        Console.WriteLine($"\n[Уведомление для {user.Name}] {notification.Message}");
        Console.WriteLine($"В документе: {notification.Document?.Title}");
        Console.WriteLine($"Изменено: {notification.Timestamp:g}");
        Console.WriteLine($"Автор изменений: {notification.User.Name}\n");
    }


    private void LoadNotificationHistory() {
        if (!File.Exists(NotificationsFile)) return;

        try {
            string json = File.ReadAllText(NotificationsFile);
            _notificationHistory = JsonSerializer.Deserialize<List<DocumentNotification>>(json)
                                   ?? [];
        }
        catch {
            _notificationHistory = [];
        }
    }

    private void SaveNotificationHistory() {
        string json = JsonSerializer.Serialize(_notificationHistory);
        File.WriteAllText(NotificationsFile, json);
    }
}
