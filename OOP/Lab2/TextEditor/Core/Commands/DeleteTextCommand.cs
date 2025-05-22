using TextEditor.Core.Documents;

namespace TextEditor.Core.Commands;

public sealed class DeleteTextCommand(Document document, int start, int length) : TextCommand(document) {
    private string? _deletedText;

    public override void Execute()
    {
        _deletedText = Document.Content?.Substring(start, length);
        Document.DeleteText(start, length);
    }

    public override void Undo()
    {
        Document.InsertText(_deletedText, start);
    }
}
