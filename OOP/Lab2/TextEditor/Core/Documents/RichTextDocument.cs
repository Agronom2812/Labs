using TextEditor.Core.Notifications;

namespace TextEditor.Core.Documents;

public sealed class RichTextDocument(INotificationService notificationService) : Document(notificationService) {
    private List<TextFormat> Formats { get; } = [];

    public override void Display() {
        Console.WriteLine($"=== {Title} (Rich Text) ===");

        if (string.IsNullOrEmpty(Content)) {
            Console.WriteLine("Документ пуст");
            return;
        }

        Console.WriteLine("Форматированное содержимое:");
        Console.ForegroundColor = ConsoleColor.Gray;

        int position = 0;
        while (position < Content.Length) {
            var formatsAtPosition = Formats
                .Where(f => f.Start <= position && position < f.End)
                .ToList();

            foreach (var format in formatsAtPosition) {
                if (format.IsBold) Console.Write("\x1B[1m");
                if (format.IsItalic) Console.Write("\x1B[3m");
                if (format.IsUnderline) Console.Write("\x1B[4m");
            }

            Console.Write(Content[position]);

            if (formatsAtPosition.Any()) {
                Console.Write("\x1B[0m");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            position++;
        }

        Console.ResetColor();
        Console.WriteLine($"\n\nФорматированных сегментов: {Formats.Count}");
    }

    public void ApplyBold(int start, int length) {
        ValidateTextRange(start, length);
        Formats.Add(new TextFormat { Start = start, End = start + length, IsBold = true });
    }

    public void ApplyItalic(int start, int length) {
        ValidateTextRange(start, length);
        Formats.Add(new TextFormat { Start = start, End = start + length, IsItalic = true });
    }

    public void ApplyUnderline(int start, int length) {
        ValidateTextRange(start, length);
        Formats.Add(new TextFormat { Start = start, End = start + length, IsUnderline = true });
    }

    private void ValidateTextRange(int start, int length) {
        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start), start,
                "Начальная позиция не может быть отрицательной");

        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), length, "Длина должна быть положительной");

        if (Content != null && start >= Content.Length)
            throw new ArgumentOutOfRangeException(nameof(start), start,
                $"Начальная позиция {start} превышает длину текста {Content.Length}");

        if (Content != null && start + length > Content.Length)
            throw new ArgumentOutOfRangeException(nameof(length), length,
                $"Конечная позиция {start + length} превышает длину текста {Content.Length}");
    }

    public override void InsertText(string? text, int position) {
        ArgumentNullException.ThrowIfNull(text);

        if (Content != null && (position < 0 || position > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(position), position,
                $"Позиция должна быть в диапазоне [0, {Content.Length}]");

        base.InsertText(text, position);

        foreach (var format in Formats) {
            if (format.Start >= position)
                format.Start += text.Length;
            if (format.End >= position)
                format.End += text.Length;
        }
    }

    public override void DeleteText(int start, int length) {
        if (Content != null && (start < 0 || start >= Content.Length))
            throw new ArgumentOutOfRangeException(nameof(start), start,
                $"Начальная позиция должна быть в диапазоне [0, {Content.Length - 1}]");

        if (Content != null && (length <= 0 || start + length > Content.Length))
            throw new ArgumentOutOfRangeException(nameof(length), length,
                $"Некорректная длина для удаления при стартовой позиции {start}");

        base.DeleteText(start, length);

        Formats.RemoveAll(f => f.Start >= start && f.End <= start + length);

        foreach (var format in Formats.ToList()) {
            if (format.Start > start + length) {
                format.Start -= length;
                format.End -= length;
            }
            else if (format.Start > start || format.End > start) {
                format.End = Math.Max(format.End - length, format.Start);
            }
        }
    }
}

public sealed class TextFormat {
    public int Start { get; set; }
    public int End { get; set; }
    public bool IsBold { get; init; }
    public bool IsItalic { get; init; }
    public bool IsUnderline { get; init; }
}
