namespace TextEditor.Core.Commands;

public sealed class CommandHistory {
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();

    public void Execute(ICommand command) {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    public bool Undo() {
        if (_undoStack.Count == 0) return false;

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        return true;
    }

    public bool Redo() {
        if (_redoStack.Count == 0) return false;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        return true;
    }

    public void Clear() {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
