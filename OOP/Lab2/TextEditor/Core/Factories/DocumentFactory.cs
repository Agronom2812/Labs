using TextEditor.Core.Documents;

namespace TextEditor.Core.Factories;

public class DocumentFactory
{
    public Document CreateDocument(string type) {
        return type.ToLower() switch {
            "plaintext" => new PlainTextDocument(),
            "markdown" => new MarkdownDocument(),
            "richtext" => new RichTextDocument(),
            _ => throw new ArgumentException("Invalid document type")
        };
    }
}
