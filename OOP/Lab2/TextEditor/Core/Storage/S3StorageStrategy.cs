using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;
using TextEditor.Core.Serialization;

namespace TextEditor.Core.Storage;

public sealed class S3StorageStrategy : IStorageStrategy {
    private readonly S3StorageService _s3Service;
    private readonly IDocumentSerializer _serializer;
    private readonly INotificationService _notificationService;

    public S3StorageStrategy(S3StorageService s3Service,
        IDocumentSerializer serializer,
        INotificationService notificationService) {
        _s3Service = s3Service;
        _serializer = serializer;
        _notificationService = notificationService;

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("./appsettings.json", optional: true);

        configBuilder.Build();
    }

    public void Save(Document document, string path) {
        try {
            string? content = _serializer.Serialize(document);
            if (content != null) {
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
                _s3Service.UploadAsync(path, stream).Wait();
            }

            _notificationService.Notify(document, $"Документ сохранен в S3: {path}");
        }
        catch (Exception ex) {
            _notificationService.Notify(document, $"Ошибка сохранения в S3: {ex.Message}");
            throw;
        }
    }

    public Document Load(string path) {
        const string bucketName = "text-ditor-bucket";

        try {
            string key = path.Replace($"s3://{bucketName}/", "").TrimStart('/');

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("S3 file key cannot be empty");

            Console.WriteLine($"Загрузка из S3. Bucket: {bucketName}, Key: {key}");

            using var response = _s3Service.DownloadAsync(bucketName, key).Result;
            using var reader = new StreamReader(response.ResponseStream);
            string content = reader.ReadToEnd();

            var document = _serializer.Deserialize(content);
            Debug.Assert(document != null, nameof(document) + " != null");
            document.Title = Path.GetFileName(key);

            _notificationService.Notify(document, $"Loaded from S3: {key}");
            return document;
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка загрузки из S3. Bucket: {bucketName}, Error: {ex.Message}");
            throw;
        }
    }

    public void Delete(string path) {
        try {
            _s3Service.DeleteAsync(path).Wait();
            _notificationService.Notify(null, $"Документ удален из S3: {path}");
        }
        catch (Exception ex) {
            _notificationService.Notify(null, $"Ошибка удаления из S3: {ex.Message}");
            throw;
        }
    }

    public bool Exists(string path) {
        try {
            return _s3Service.ExistsAsync(path).Result;
        }
        catch {
            return false;
        }
    }
}
