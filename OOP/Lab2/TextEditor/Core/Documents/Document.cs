using TextEditor.Core.Factories;
using TextEditor.Core.Notifications;
using TextEditor.Core.Users;
using TextEditor.Services;

namespace TextEditor.Core.Documents;

public abstract class Document {

    private readonly INotificationService _notificationService;

    protected Document(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    public string? Title { get; set; } = "Новый документ";
    public string? Content { get; set; } = string.Empty;

    public void Copy(int start, int length)
    {
        if (Content != null && (start < 0 || start >= Content.Length))
                throw new ArgumentOutOfRangeException(nameof(start), $"Позиция {start} вне диапазона" +
                                                                     $" [0, {Content.Length - 1}]");

        if (Content != null && (length < 0 || start + length > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(length), $"Длина {length} некорректна для позиции {start}");

        ClipboardService.Copy(Content?.Substring(start, length), start, length);
    }

    public void Cut(int start, int length)
    {
        Copy(start, length);
        DeleteText(start, length);
    }

    public void Paste(int position)
    {
        InsertText(ClipboardService.Paste(), position);
    }

    public static bool Exists(string path)
    {
        return File.Exists(path);
    }


    public virtual void InsertText(string? text, int position)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (Content != null && (position < 0 || position > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(position));

        Content = Content?.Insert(position, text);
        _notificationService.Notify(this, $"Добавлен текст: '{text}' на позицию {position}");
    }

    public virtual void DeleteText(int start, int length)
    {
        if (Content != null && (start < 0 || start >= Content.Length))
            throw new ArgumentOutOfRangeException(nameof(start));

        if (Content != null && (length < 0 || start + length > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(length));

        string deletedText = Content?.Substring(start, length) ?? string.Empty;
        Content = Content?.Remove(start, length);
        _notificationService.Notify(this,
            $"Удален текст: '{deletedText}' (позиция {start}, длина {length})");
    }

    public void Save(string filePath, INotificationService notificationService)
    {
        var serializer = SerializerFactory.GetSerializer(filePath, notificationService);
        File.WriteAllText(
            EnsureExtension(filePath, serializer.FileExtension),
            serializer.Serialize(this)
        );
    }

    public static Document? Load(string filePath, INotificationService notificationService)
    {
        var serializer = SerializerFactory.GetSerializer(filePath, notificationService);
        return serializer.Deserialize(File.ReadAllText(filePath));
    }

    public abstract void Display();

    private static string EnsureExtension(string path, string extension)
        => path.EndsWith(extension) ? path : path + extension;


}
