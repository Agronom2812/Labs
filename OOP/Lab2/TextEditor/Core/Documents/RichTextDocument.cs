using System.Text.Json;

namespace TextEditor.Core.Documents;

public class RichTextDocument : Document
{
    private List<TextFormat> Formats { get; set; } = [];

    public override void Display()
    {
        Console.WriteLine($"✨ {Title} (Rich Text) ✨");

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

    public override void Save(string filePath)
    {
        if (!filePath.EndsWith(".rtf"))
            filePath += ".rtf";

        var data = new {
            Content,
            Formats
        };

        File.WriteAllText(filePath, JsonSerializer.Serialize(data));
        Console.WriteLine($"[RichText] Saved to {Path.GetFullPath(filePath)}");
    }

    public override void Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Document not found", filePath);

        dynamic? data = JsonSerializer.Deserialize<dynamic>(File.ReadAllText(filePath));
        if (data != null) {
            Content = data.GetProperty("Content").GetString();
            Formats = JsonSerializer.Deserialize<List<TextFormat>>(data.GetProperty("Formats").GetRawText());
        }

        Title = Path.GetFileNameWithoutExtension(filePath);
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

