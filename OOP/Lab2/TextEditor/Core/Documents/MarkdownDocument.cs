using TextEditor.Core.Notifications;

namespace TextEditor.Core.Documents;

public sealed class MarkdownDocument(INotificationService notificationService) : Document(notificationService) {
    private static readonly char[] s_separator = ['\n', '\r'];

    public override void Display() {
        if (string.IsNullOrEmpty(Content)) {
            Console.WriteLine("--- Документ пуст ---");
            return;
        }

        Console.WriteLine($"--- {Title} (Markdown) ---");

        try {
            string[] lines = Content.Split(s_separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines) {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("# ")) {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Заголовок: {line.Substring(2)}");
                    Console.ResetColor();
                }
                else if (line.StartsWith("**") && line.EndsWith("**")) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Жирный: {line.Substring(2, line.Length - 4)}");
                    Console.ResetColor();
                }
                else if (line.StartsWith('*') && line.EndsWith('*') && line.Length > 1) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Курсив: {line.Substring(1, line.Length - 2)}");
                    Console.ResetColor();
                }
                else {
                    Console.WriteLine(line);
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка отображения: {ex.Message}");
            Console.WriteLine("Исходное содержимое:");
            Console.WriteLine(Content);
        }
    }

    public override void InsertText(string? text, int position) {
        ArgumentNullException.ThrowIfNull(text);

        base.InsertText(text, position);
        Console.WriteLine($"Добавлен markdown-текст: {text}");
    }

    public override void DeleteText(int start, int length) {
        string? deletedPart = Content?.Substring(start, length);
        if (deletedPart != null && (deletedPart.Contains('#')
                                    || deletedPart.Contains('*') || deletedPart.Contains('_'))) {
            Console.WriteLine("Предупреждение: удаление markdown-разметки может нарушить форматирование!");
        }

        base.DeleteText(start, length);
        Console.WriteLine($"Удалено {length} символов");
    }
}
