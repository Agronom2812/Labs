using ConsolePaint.Shapes;
using SkiaSharp;

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
public sealed class MoveCommand(IShape shape, float dx, float dy) : ICommand {
    private bool _isFirstExecution = true;
    private SKPoint _originalPosition = shape.Center;

    public void Execute()
    {
        if (_isFirstExecution)
        {
            shape.Move(dx, dy);
            _isFirstExecution = false;
        }
        else
        {
            shape.Move(dx, dy);
        }
    }

    public void Undo()
    {
        shape.Move(-dx, -dy);
    }
}
