using TextEditor.Core.Notifications;
using TextEditor.Core.Services;

namespace TextEditor.Core.Documents;

public abstract class Document(INotificationService notificationService) {
    public string? Title { get; set; } = "Новый документ";
    public string? Content { get; set; } = string.Empty;

    public void Copy(int start, int length) {
        if (Content != null && (start < 0 || start >= Content.Length))
            throw new ArgumentOutOfRangeException(nameof(start), $"Позиция {start} вне диапазона" +
                                                                 $" [0, {Content.Length - 1}]");

        if (Content != null && (length < 0 || start + length > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(length), $"Длина {length} некорректна для позиции {start}");

        ClipboardService.Copy(Content?.Substring(start, length));
    }

    public void Cut(int start, int length) {
        Copy(start, length);
        DeleteText(start, length);
    }

    public void Paste(int position) {
        InsertText(ClipboardService.Paste(), position);
    }


    public virtual void InsertText(string? text, int position) {
        ArgumentNullException.ThrowIfNull(text);

        if (Content != null && (position < 0 || position > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(position));

        Content = Content?.Insert(position, text);
        notificationService.Notify(this, $"Добавлен текст: '{text}' на позицию {position}");
    }

    public virtual void DeleteText(int start, int length) {
        if (Content != null && (start < 0 || start >= Content.Length))
            throw new ArgumentOutOfRangeException(nameof(start));

        if (Content != null && (length < 0 || start + length > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(length));

        string deletedText = Content?.Substring(start, length) ?? string.Empty;
        Content = Content?.Remove(start, length);
        notificationService.Notify(this,
            $"Удален текст: '{deletedText}' (позиция {start}, длина {length})");
    }

    public abstract void Display();
}
