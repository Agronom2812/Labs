using TextEditor.Core.Factories;
using TextEditor.Services;

namespace TextEditor.Core.Documents;

public abstract class Document {
    public string? Title { get; set; } = "Новый документ";
    public string? Content { get; set; } = string.Empty;
    protected string ClipboardBuffer = string.Empty;

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

    public void ApplyFormat(int start, int length, Func<string, string> formatter)
    {
        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter), "Функция форматирования не может быть null");
        }

        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), start,
                $"Начальная позиция {start} не может быть отрицательной");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length,
                $"Длина {length} не может быть отрицательной");
        }

        if (Content != null && start >= Content.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(start), start,
                $"Начальная позиция {start} превышает длину текста ({Content.Length})");
        }

        if (Content != null && start + length > Content.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length,
                $"Суммарная длина {start + length} превышает длину текста ({Content.Length})");
        }

        try
        {
            string? selectedText = Content?.Substring(start, length);

            if (selectedText == null) return;

            string formattedText = formatter(selectedText);

            Content = Content?.Remove(start, length).Insert(start, formattedText);
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or ArgumentException or NullReferenceException)
        {
            Console.Error.WriteLine($"Ошибка форматирования: {ex.Message}");
            throw;
        }
    }

    public virtual bool Exists(string path)
    {
        return File.Exists(path);
    }


    public virtual void InsertText(string? text, int position) {
        ArgumentNullException.ThrowIfNull(text);

        if (Content != null && (position < 0 || position > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(position), "Position is out of range");

        Content = Content?.Insert(position, text);
    }

    public virtual void DeleteText(int start, int length)
    {
        if (Content != null && (start < 0 || start >= Content.Length))
            throw new ArgumentOutOfRangeException(nameof(start), "Start position is invalid");

        if (Content != null && (length < 0 || start + length > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(length), "Invalid length");

        Content = Content?.Remove(start, length);
    }

    public void Save(string filePath)
    {
        var serializer = SerializerFactory.GetSerializer(filePath);
        File.WriteAllText(
            EnsureExtension(filePath, serializer.FileExtension),
            serializer.Serialize(this)
        );
    }

    public static Document? Load(string filePath)
    {
        var serializer = SerializerFactory.GetSerializer(filePath);
        return serializer.Deserialize(File.ReadAllText(filePath));
    }

    public abstract void Display();

    private static string EnsureExtension(string path, string extension)
        => path.EndsWith(extension) ? path : path + extension;


}
