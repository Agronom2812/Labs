using ConsolePaint.Shapes;
using System.Collections.Generic;

namespace ConsolePaint.Commands;

public class DrawCommand(IList<IShape> shapes, IShape shape) : ICommand {
    public void Execute()
    {
        shapes.Add(shape);
    }

    public void Undo()
    {
        shapes.Remove(shape);
    }
}
