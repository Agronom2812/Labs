namespace TextEditor.Core.Documents;

public class MarkdownDocument : Document
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

    public override void Save(string filePath)
    {
        if (!filePath.EndsWith(".md"))
            filePath += ".md";

        File.WriteAllText(filePath, Content);
        Console.WriteLine($"[Markdown] Saved to {Path.GetFullPath(filePath)}");
    }

    public override void Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Document not found", filePath);

        Content = File.ReadAllText(filePath);
        Title = Path.GetFileNameWithoutExtension(filePath);
    }
}
