namespace TextEditor.Core.Documents;

public sealed class PlainTextDocument : Document {

    public override void Display()
    {
        Console.WriteLine($"=== {Title} (Plain Text) ===");
        Console.WriteLine(Content);
        Console.WriteLine($"=== Length: {Content.Length} chars ===");
    }

    public override void InsertText(string? text, int position)
    {
        base.InsertText(text, position);
        Console.WriteLine($"Inserted {text.Length} chars at position {position}");
    }

    public override void DeleteText(int start, int length)
    {
        base.DeleteText(start, length);
        Console.WriteLine($"Deleted {length} chars from position {start}");
    }

}
