namespace TextEditor.Core.Documents;

public sealed class PlainTextDocument : Document {

    public override void Display()
    {
        Console.WriteLine($"=== {Title} (Plain Text) ===");
        Console.WriteLine(Content);
        if (Content != null) Console.WriteLine($"=== Length: {Content.Length} chars ===");
    }

    public override void InsertText(string? text, int position)
    {
        Console.WriteLine($"Вставляем текст: '{text}' на позицию {position}");
        base.InsertText(text, position);
        Console.WriteLine($"Текст успешно вставлен. Текущее содержимое:\n{Content}");
    }

    public override void DeleteText(int start, int length)
    {
        Console.WriteLine($"Удаляем {length} символов с позиции {start}");
        base.DeleteText(start, length);
        Console.WriteLine($"Текст успешно удален. Текущее содержимое:\n{Content}");
    }
}
