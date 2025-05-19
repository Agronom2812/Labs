using ConsolePaint.Commands;
using ConsolePaint.Shapes;
using SkiaSharp;
using Xunit;

namespace ConsolePaint.Tests.CommandTests;

public sealed class ReplaceAllCommandTests
{
    [Fact]
    public void Execute_ReplacesAllShapes()
    {
        // Given
        var oldShapes = new List<IShape> { new Circle(new SKPoint(10, 10), 5) };
        var newShapes = new List<IShape> { new Rectangle(new SKPoint(20, 20), 10, 10) };
        var command = new ReplaceAllCommand(oldShapes, newShapes);

        // When
        command.Execute();

        // Then
        Assert.Single(oldShapes);
        Assert.IsType<Rectangle>(oldShapes[0]);
    }

    [Fact]
    public void Undo_RestoresOriginalShapes()
    {
        // Given
        var oldShapes = new List<IShape> { new Circle(new SKPoint(10, 10), 5) };
        int originalCount = oldShapes.Count;
        var command = new ReplaceAllCommand(oldShapes, []);
        command.Execute();

        // When
        command.Undo();

        // Then
        Assert.Equal(originalCount, oldShapes.Count);
        Assert.IsType<Circle>(oldShapes[0]);
    }
}
