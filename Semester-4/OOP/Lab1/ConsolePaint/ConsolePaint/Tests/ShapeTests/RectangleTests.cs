using ConsolePaint.Shapes;
using SkiaSharp;
using Xunit;

namespace ConsolePaint.Tests.ShapeTests;

public sealed class RectangleTests
{
    [Theory]
    [InlineData(10, 10, 20, 30)] // Обычный прямоугольник
    [InlineData(0, 0, 5, 5)]     // Квадрат
    public void Constructor_SetsCorrectDimensions(float x, float y, float width, float height)
    {
        var rect = new Rectangle(new SKPoint(x, y), width, height);

        Assert.Equal(width, rect.Width);
        Assert.Equal(height, rect.Height);
        Assert.Equal(new SKPoint(x, y), rect.Center);
    }

    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(15, 15, true)]
    [InlineData(20, 20, false)]
    public void Contains_ReturnsCorrectResult(float testX, float testY, bool expected)
    {
        // Given
        var rect = new Rectangle(new SKPoint(10, 10), 10, 10);

        // When
        bool result = rect.Contains(new SKPoint(testX, testY));

        // Then
        Assert.Equal(expected, result);
    }
}
