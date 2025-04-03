using ConsolePaint.Shapes;

namespace ConsolePaint.Commands;

public sealed class ClearAllCommand : ICommand
{
    private readonly List<IShape> _shapes;
    private readonly List<IShape> _backup = [];

    public ClearAllCommand(List<IShape> shapes)
    {
        _shapes = shapes;
        _backup.AddRange(shapes);
    }

    public void Execute()
    {
        _shapes.Clear();
    }

    public void Undo()
    {
        _shapes.AddRange(_backup);
    }
}
