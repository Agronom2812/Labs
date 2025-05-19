namespace TextEditor.Core.Documents;

public class PlainTextDocument : Document {
    public override void Save(string filePath)
    {
        if (!filePath.EndsWith(".txt"))
            filePath += ".txt";

        File.WriteAllText(filePath, Content);
        Console.WriteLine($"[PlainText] Saved to {Path.GetFullPath(filePath)}");
    }

    public override void Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Document not found", filePath);

        Content = File.ReadAllText(filePath);
        Title = Path.GetFileNameWithoutExtension(filePath);
    }

    public override void Display()
    {
        Console.WriteLine($"=== {Title} (Plain Text) ===");
        Console.WriteLine(Content);
        Console.WriteLine($"=== Length: {Content.Length} chars ===");
    }
}
