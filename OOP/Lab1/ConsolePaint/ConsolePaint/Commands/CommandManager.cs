namespace ConsolePaint.Commands;

/// <summary>
/// Manages command execution history for undo/redo operations.
/// </summary>
/// <remarks>
/// Maintains two stacks:
/// <list type="bullet">
/// <item><description>_undoStack - for executed commands</description></item>
/// <item><description>_redoStack - for undone commands</description></item>
/// </list>
/// Implements the command history pattern.
/// </remarks>
public sealed class CommandManager
{
    private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    /// <summary>
    /// Executes a command and adds it to the history.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    public void Execute(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    /// <summary>
    /// Undoes the most recent command.
    /// </summary>
    public void Undo() {
        if (_undoStack.Count <= 0) return;

        var cmd = _undoStack.Pop();
        cmd.Undo();
        _redoStack.Push(cmd);
    }

    /// <summary>
    /// Redoes the most recently undone command.
    /// </summary>
    public void Redo() {
        if (_redoStack.Count <= 0) return;

        var cmd = _redoStack.Pop();
        cmd.Execute();
        _undoStack.Push(cmd);
    }
}
