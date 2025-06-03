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
public sealed class MoveCommand(IShape shape, float dx, float dy) : ICommand
{
    private SKPoint _originalCenter = shape.Center;
    private bool _wasExecuted = false;

    public void Execute()
    {
        if (!_wasExecuted)
        {
            _originalCenter = shape.Center;
            _wasExecuted = true;
        }

        shape.Move(dx, dy);
    }

    public void Undo()
    {
        shape.Center = _originalCenter;
    }
}
