using ConsolePaint.Shapes;

namespace ConsolePaint.Commands;

/// <summary>
/// Represents a command for removing shapes from the canvas.
/// </summary>
/// <param name="shapes">The collection of shapes to modify.</param>
/// <param name="shape">The shape to remove.</param>
/// <remarks>
/// Stores the removal position for accurate undo operations.
/// Implements <see cref="ICommand"/> for undo/redo functionality.
/// </remarks>
public sealed class EraseCommand(IList<IShape> shapes, IShape shape) : ICommand {
    private int _index;

    /// <summary>
    /// Executes the erase command by removing the shape.
    /// </summary>
    public void Execute()
    {
        _index = shapes.IndexOf(shape);
        if (_index < 0) return;

        shapes.RemoveAt(_index);
    }

    /// <summary>
    /// Reverts the erase command by restoring the shape.
    /// </summary>
    public void Undo() {
        if (_index < 0) return;

        shapes.Insert(_index, shape);
    }
}
