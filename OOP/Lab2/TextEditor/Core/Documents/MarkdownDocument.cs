namespace TextEditor.Core.Documents;

public sealed class MarkdownDocument : Document
{
    public override void Display()
    {
        Console.WriteLine($"--- {Title} (Markdown) ---");

        string[] lines = Content.Split('\n');
        foreach (string line in lines)
        {
            if (line.StartsWith("# "))
                Console.WriteLine($"\n[HEADER] {line[2..]}");
            else if (line.StartsWith("**") && line.EndsWith("**"))
                Console.WriteLine($"[BOLD] {line[2..^2]}");
            else
                Console.WriteLine(line);
        }
    }

    public override void InsertText(string? text, int position)
    {
        if (position == 0 && string.IsNullOrWhiteSpace(Content))
            text = "# " + text;

        base.InsertText(text, position);
    }
}
