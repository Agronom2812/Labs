using TextEditor.Core.Documents;

namespace TextEditor.Core.Commands;

public abstract class TextCommand : ICommand
{
    protected readonly Document _document;
    private readonly string? _previousContent;

    protected TextCommand(Document document)
    {
        _document = document;
        _previousContent = document.Content;
    }

    public abstract void Execute();

    public virtual void Undo()
    {
        _document.Content = _previousContent;
    }
}
