using TextEditor.Core.Documents;
using TextEditor.Core.Factories;
using TextEditor.Core.Notifications;
using TextEditor.Core.Serialization;

namespace TextEditor.Core.Storage;

public sealed class CloudStorage : IStorageStrategy
{
    private readonly INotificationService _notificationService;

    public CloudStorage(INotificationService notificationService) {
        _notificationService = notificationService;
    }
    public void Save(Document document, string path)
    {
        var serializer = SerializerFactory.GetSerializer(path, _notificationService);
        string? content = serializer.Serialize(document);

        Console.WriteLine($"Uploading to cloud: {path}");
    }

    public Document? Load(string path)
    {
        Console.WriteLine($"Downloading from cloud: {path}");
        var serializer = SerializerFactory.GetSerializer(path, _notificationService);
        return serializer.Deserialize("");
    }

    public void Delete(string path) =>
        Console.WriteLine($"Deleting from cloud: {path}");

    public bool Exists(string path)
    {
        Console.WriteLine($"Checking existence in cloud: {path}");
        return true;
    }
}
