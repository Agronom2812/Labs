using TextEditor.Core.Documents;

namespace TextEditor.Core.Commands;

public abstract class TextCommand(Document document) : ICommand {
    protected readonly Document Document = document;
    private readonly string? _previousContent = document.Content;

    public abstract void Execute();

    public virtual void Undo()
    {
        Document.Content = _previousContent;
    }
}
