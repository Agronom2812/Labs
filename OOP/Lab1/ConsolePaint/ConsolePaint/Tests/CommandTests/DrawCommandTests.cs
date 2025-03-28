using ConsolePaint.Commands;
using ConsolePaint.Shapes;
using SkiaSharp;
using Xunit;

namespace ConsolePaint.Tests.CommandTests;

public sealed class DrawCommandTests
{
    public static IEnumerable<object[]> ShapeTestData =>
        new List<object[]>
        {
            new object[] { new Circle(new SKPoint(10, 10), 5) },
            new object[] { new Rectangle(new SKPoint(20, 20), 10, 10) },
            new object[] { new Line(new SKPoint(0, 0), new SKPoint(10, 10)) }
        };

    [Theory]
    [MemberData(nameof(ShapeTestData))]
    public void Execute_AddsShapeToList(IShape shape)
    {
        // Given
        var shapes = new List<IShape>();
        var command = new DrawCommand(shapes, shape);

        // When
        command.Execute();

        // Then
        Assert.Single(shapes);
        Assert.Equal(shape, shapes[0]);
    }

    [Theory]
    [MemberData(nameof(ShapeTestData))]
    public void Undo_RemovesShapeFromList(IShape shape)
    {
        // Given
        var shapes = new List<IShape>();
        var command = new DrawCommand(shapes, shape);
        command.Execute();

        // When
        command.Undo();

        // Then
        Assert.Empty(shapes);
    }
}
