using TextEditor.Core.Documents;
using TextEditor.Core.Factories;
using TextEditor.Core.Notifications;

namespace TextEditor.Core.Storage;

public sealed class CloudStorage(INotificationService notificationService) : IStorageStrategy {
    public void Save(Document document, string path) {
        var serializer = SerializerFactory.GetSerializer(path, notificationService);
        serializer.Serialize(document);

        Console.WriteLine($"Uploading to cloud: {path}");
    }

    public Document? Load(string path) {
        Console.WriteLine($"Downloading from cloud: {path}");
        var serializer = SerializerFactory.GetSerializer(path, notificationService);
        return serializer.Deserialize("");
    }

    public void Delete(string path) =>
        Console.WriteLine($"Deleting from cloud: {path}");
}
