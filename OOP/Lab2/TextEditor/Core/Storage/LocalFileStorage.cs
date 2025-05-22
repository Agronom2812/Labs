using TextEditor.Core.Documents;
using TextEditor.Core.Factories;
using TextEditor.Core.Serialization;
using TextEditor.Core.Notifications;

namespace TextEditor.Core.Storage;

public sealed class LocalFileStorage(INotificationService notificationService) : IStorageStrategy {
    public void Save(Document document, string path) {
        var serializer = SerializerFactory.GetSerializer(path, notificationService);
        File.WriteAllText(path, serializer.Serialize(document));
    }

    public Document? Load(string path) {
        var serializer = SerializerFactory.GetSerializer(path, notificationService);
        return serializer.Deserialize(File.ReadAllText(path));
    }

    public void Delete(string path) => File.Delete(path);

    public bool Exists(string path) => File.Exists(path);
}
