using ConsolePaint.Shapes;

namespace ConsolePaint.Commands
{
    public sealed class ReplaceAllCommand : ICommand
    {
        private readonly List<IShape> _target;
        private readonly List<IShape> _newShapes;
        private readonly List<IShape> _oldShapes = [];

        public ReplaceAllCommand(List<IShape> target, List<IShape?> newShapes)
        {
            _target = target;
            _newShapes = newShapes;
            _oldShapes.AddRange(target);
        }

        public void Execute()
        {
            _target.Clear();
            _target.AddRange(_newShapes);
        }

        public void Undo()
        {
            _target.Clear();
            _target.AddRange(_oldShapes);
        }
    }
}
