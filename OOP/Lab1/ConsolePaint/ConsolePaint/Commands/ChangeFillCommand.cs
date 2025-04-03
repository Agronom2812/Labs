using ConsolePaint.Shapes;
using SkiaSharp;

namespace ConsolePaint.Commands;

public sealed class ChangeFillCommand(IShape shape, SKColor newColor) : ICommand {
    private readonly SKColor _oldColor = shape.Background;

    public void Execute() => shape.Background = newColor;
    public void Undo() => shape.Background = _oldColor;
}
