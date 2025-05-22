using TextEditor.Core.Notifications;
using TextEditor.Core.Storage;

namespace TextEditor.Core.Factories;

public static class StorageStrategyFactory {
    public static IStorageStrategy Create(StorageType type, INotificationService notificationService) {
        return type switch {
            StorageType.Local => new LocalFileStorage(notificationService),
            StorageType.Cloud => new CloudStorage(notificationService),
            _ => throw new NotSupportedException($"Storage type {type} is not supported")
        };
    }
}

public enum StorageType {
    Local,
    Cloud
}
