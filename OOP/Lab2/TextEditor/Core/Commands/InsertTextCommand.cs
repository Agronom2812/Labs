using TextEditor.Core.Documents;

namespace TextEditor.Core.Commands;

public sealed class InsertTextCommand(Document document, string text, int position) : TextCommand(document) {
    public override void Execute()
    {
        Document.InsertText(text, position);
    }

    public override void Undo()
    {
        base.Undo();
        Document.DeleteText(position, text.Length);
    }
}
