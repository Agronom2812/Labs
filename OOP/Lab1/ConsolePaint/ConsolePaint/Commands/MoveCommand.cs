using ConsolePaint.Shapes;

namespace ConsolePaint.Commands;

public class MoveCommand(IShape shape, float dx, float dy) : ICommand {
    private bool _isExecuted = false;

    public void Execute() {
        if (_isExecuted) return;

        shape.Move(dx, dy);
        _isExecuted = true;
    }

    public void Undo() {
        if (!_isExecuted) return;

        shape.Move(-dx, -dy);
        _isExecuted = false;
    }
}
