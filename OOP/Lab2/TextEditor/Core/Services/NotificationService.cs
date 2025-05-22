using System.Text.Json;
using TextEditor.Core.Documents;
using TextEditor.Core.Users;

namespace TextEditor.Core.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly Dictionary<Document, List<User>> _documentSubscriptions = new();
    private List<DocumentNotification> _notificationHistory = new();
    private const string NotificationsFile = "notifications.json";
    private readonly UserManager _userManager;

    public NotificationService(UserManager userManager)
    {
        _userManager = userManager;
        LoadNotificationHistory();
    }

    public void Subscribe(User user, Document document)
    {
        if (!_documentSubscriptions.ContainsKey(document))
        {
            _documentSubscriptions[document] = new List<User>();
        }

        if (!_documentSubscriptions[document].Contains(user))
        {
            _documentSubscriptions[document].Add(user);
            SaveNotificationHistory();
        }
    }

    public void Unsubscribe(User user, Document document)
    {
        if (_documentSubscriptions.ContainsKey(document))
        {
            _documentSubscriptions[document].Remove(user);
            SaveNotificationHistory();
        }
    }

    public void Notify(Document document, string message)
    {
        if (!_documentSubscriptions.ContainsKey(document)) return;

        var notification = new DocumentNotification(document, message);
        _notificationHistory.Add(notification);
        SaveNotificationHistory();

        foreach (var user in _documentSubscriptions[document])
        {
            if (user != _userManager.CurrentUser)
            {
                SendNotification(user, document, notification);
            }
        }
    }

    private void SendNotification(User user, Document document, DocumentNotification notification)
    {
        Console.WriteLine($"\n[Уведомление для {user.Name}] {notification.Message}");
        Console.WriteLine($"В документе: {document.Title}");
        Console.WriteLine($"Изменено: {notification.Timestamp:g}");
        Console.WriteLine($"Автор изменений: {_userManager.CurrentUser?.Name ?? "Система"}\n");
    }

    public IEnumerable<DocumentNotification> GetUserNotifications(User user)
    {
        return _notificationHistory
            .Where(n => n.User == user)
            .OrderByDescending(n => n.Timestamp);
    }

    private void LoadNotificationHistory()
    {
        if (File.Exists(NotificationsFile))
        {
            try
            {
                string json = File.ReadAllText(NotificationsFile);
                _notificationHistory = JsonSerializer.Deserialize<List<DocumentNotification>>(json)
                    ?? new List<DocumentNotification>();
            }
            catch
            {
                _notificationHistory = new List<DocumentNotification>();
            }
        }
    }

    private void SaveNotificationHistory()
    {
        string json = JsonSerializer.Serialize(_notificationHistory);
        File.WriteAllText(NotificationsFile, json);
    }
}
