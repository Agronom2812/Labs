using ConsolePaint.Shapes;
using System.Collections.Generic;

namespace ConsolePaint.Commands;

public class EraseCommand(IList<IShape> shapes, IShape shape) : ICommand {
    private int _index;

    public void Execute()
    {
        _index = shapes.IndexOf(shape);
        if (_index < 0) return;

        shapes.RemoveAt(_index);
    }

    public void Undo() {
        if (_index < 0) return;

        shapes.Insert(_index, shape);
    }
}
