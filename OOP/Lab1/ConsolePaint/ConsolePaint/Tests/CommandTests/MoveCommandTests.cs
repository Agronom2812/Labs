using ConsolePaint.Commands;
using ConsolePaint.Shapes;
using SkiaSharp;
using Xunit;

namespace ConsolePaint.Tests.CommandTests;

public sealed class MoveCommandTests
{
    [Theory]
    [InlineData(5, 5)]
    [InlineData(-3, -3)]
    public void Execute_MovesShapeCorrectly(float dx, float dy)
    {
        // Given
        var shape = new Circle(new SKPoint(10, 10), 5);
        var originalCenter = shape.Center;
        var command = new MoveCommand(shape, dx, dy);

        // When
        command.Execute();

        // Then
        Assert.Equal(originalCenter.X + dx, shape.Center.X);
        Assert.Equal(originalCenter.Y + dy, shape.Center.Y);
    }

    [Fact]
    public void Undo_ReturnsShapeToOriginalPosition()
    {
        // Given
        var shape = new Circle(new SKPoint(10, 10), 5);
        var originalCenter = shape.Center;
        var command = new MoveCommand(shape, 5, 5);
        command.Execute();

        // When
        command.Undo();

        // Then
        Assert.Equal(originalCenter, shape.Center);
    }
}
