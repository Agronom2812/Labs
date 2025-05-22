using TextEditor.Core.Documents;

namespace TextEditor.Core.Commands;

public class DeleteTextCommand : TextCommand
{
    private readonly int _start;
    private readonly int _length;
    private string _deletedText;

    public DeleteTextCommand(Document document, int start, int length)
        : base(document)
    {
        _start = start;
        _length = length;
    }

    public override void Execute()
    {
        _deletedText = _document.Content.Substring(_start, _length);
        _document.DeleteText(_start, _length);
    }

    public override void Undo()
    {
        _document.InsertText(_deletedText, _start);
    }
}
