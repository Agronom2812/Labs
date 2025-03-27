using ConsolePaint.Shapes;

namespace ConsolePaint.Commands;

/// <summary>
/// Represents a command for moving shapes on the canvas.
/// </summary>
/// <param name="shape">The shape to move.</param>
/// <param name="dx">Horizontal movement offset (in pixels).</param>
/// <param name="dy">Vertical movement offset (in pixels).</param>
/// <remarks>
/// This command stores the movement delta and applies it reversibly.
/// Implements <see cref="ICommand"/> for undo/redo functionality.
/// </remarks>
public class MoveCommand(IShape shape, float dx, float dy) : ICommand {
    private bool _isExecuted = false;

    /// <summary>
    /// Executes the move command.
    /// </summary>
    public void Execute() {
        if (_isExecuted) return;

        shape.Move(dx, dy);
        _isExecuted = true;
    }

    /// <summary>
    /// Reverts the move command.
    /// </summary>
    public void Undo() {
        if (!_isExecuted) return;

        shape.Move(-dx, -dy);
        _isExecuted = false;
    }
}
