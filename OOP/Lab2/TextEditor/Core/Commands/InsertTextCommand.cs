using TextEditor.Core.Documents;

namespace TextEditor.Core.Commands;

public class InsertTextCommand : TextCommand
{
    private readonly string _text;
    private readonly int _position;

    public InsertTextCommand(Document document, string text, int position)
        : base(document)
    {
        _text = text;
        _position = position;
    }

    public override void Execute()
    {
        _document.InsertText(_text, _position);
    }

    public override void Undo()
    {
        base.Undo();
        _document.DeleteText(_position, _text.Length);
    }
}
