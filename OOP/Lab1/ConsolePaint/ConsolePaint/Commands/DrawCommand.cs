using ConsolePaint.Shapes;

namespace ConsolePaint.Commands;

/// <summary>
/// Represents a command for adding shapes to the canvas.
/// </summary>
/// <param name="shapes">The collection of shapes to modify.</param>
/// <param name="shape">The shape to add.</param>
/// <remarks>
/// Implements <see cref="ICommand"/> to support undo/redo operations.
/// Maintains references to both the shape collection and the added shape.
/// </remarks>
public sealed class DrawCommand(IList<IShape> shapes, IShape shape) : ICommand {

    /// <summary>
    /// Executes the draw command by adding the shape.
    /// </summary>
    public void Execute()
    {
        shapes.Add(shape);
    }

    /// <summary>
    /// Reverts the draw command by removing the shape.
    /// </summary>
    public void Undo()
    {
        shapes.Remove(shape);
    }
}
