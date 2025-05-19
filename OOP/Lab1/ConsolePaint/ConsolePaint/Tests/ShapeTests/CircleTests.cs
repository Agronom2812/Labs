using ConsolePaint.Shapes;
using SkiaSharp;
using Xunit;

namespace ConsolePaint.Tests.ShapeTests;

public sealed class CircleTests
{
    [Theory]
    [InlineData(10, 20, 5)]
    [InlineData(0, 0, 1)]
    [InlineData(-10, -20, 10)]
    public void Constructor_SetsProperties(float x, float y, float radius)
    {
        // Arrange & Act
        var circle = new Circle(new SKPoint(x, y), radius);

        // Assert
        Assert.Equal(x, circle.Center.X);
        Assert.Equal(y, circle.Center.Y);
        Assert.Equal(radius, circle.Radius);
    }

    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(12, 12, true)]
    [InlineData(15, 10, true)]
    [InlineData(20, 20, false)]
    public void Contains_ReturnsCorrectResult(float x, float y, bool expected)
    {
        // Given
        var circle = new Circle(new SKPoint(10, 10), 5);

        // When
        bool result = circle.Contains(new SKPoint(x, y));

        // Then
        Assert.Equal(expected, result);
    }
}
