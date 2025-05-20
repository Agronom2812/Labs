namespace TextEditor.Core.Documents;

public sealed class RichTextDocument : Document
{
    private List<TextFormat> Formats { get; } = [];

    public override void Display()
    {
        Console.WriteLine($"{Title} (Rich Text)");

        for (int i = 0; i < Content.Length; i++)
        {
            int i1 = i;
            var formats = Formats.Where(f => f.Start <= i1 && i1 < f.End);
            IEnumerable<TextFormat> textFormats = formats.ToList();
            Console.ForegroundColor = textFormats.Any(f => f.IsBold) ? ConsoleColor.White
                : ConsoleColor.Gray;

            if (textFormats.Any(f => f.IsUnderline))
                Console.Write("\x1B[4m");

            Console.Write(Content[i]);

            if (textFormats.Any(f => f.IsUnderline))
                Console.Write("\x1B[24m");
        }
        Console.ResetColor();
        Console.WriteLine($"\n🔠 Formatting: {Formats.Count} segments");
    }

    public override void InsertText(string? text, int position)
    {
        base.InsertText(text, position);
        foreach (var format in Formats)
        {
            if (format.Start >= position)
                format.Start += text.Length;
            if (format.End >= position)
                format.End += text.Length;
        }
    }
}

public abstract class TextFormat
{
    public int Start { get; set; }
    public int End { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
}

