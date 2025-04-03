using ConsolePaint.Commands;
using ConsolePaint.Shapes;
using SkiaSharp;
using Xunit;

namespace ConsolePaint.Tests.CommandTests;

public sealed class ClearAllCommandTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Execute_ClearsShapesList_RegardlessOfCount(int shapeCount)
    {
        // Given
        List<IShape> shapes = Enumerable.Range(0, shapeCount)
            .Select(_ => new Circle(new SKPoint(10, 10), 5))
            .ToList<IShape>();
        var command = new ClearAllCommand(shapes);

        // When
        command.Execute();

        // Then
        Assert.Empty(shapes);
    }

    [Fact]
    public void Undo_RestoresShapesList()
    {
        // Given
        var shapes = new List<IShape> { new Circle(new SKPoint(10, 10), 5) };
        var command = new ClearAllCommand(shapes);
        command.Execute();

        // When
        command.Undo();

        // Then
        Assert.Single(shapes);
    }
}
