using ConsolePaint.Shapes;
using SkiaSharp;
using Xunit;

namespace ConsolePaint.Tests.ShapeTests;

public sealed class LineTests
{
    public static IEnumerable<object[]> LineTestData =>
        new List<object[]>
        {
            new object[] { new SKPoint(10, 10), new SKPoint(20, 20) },
            new object[] { new SKPoint(0, 0), new SKPoint(10, 0) },
            new object[] { new SKPoint(-5, -5), new SKPoint(5, 5) }
        };

    [Theory]
    [MemberData(nameof(LineTestData))]
    public void Constructor_SetsProperties(SKPoint start, SKPoint end)
    {
        var line = new Line(start, end);

        Assert.Equal(start, line.StartPoint);
        Assert.Equal(end, line.EndPoint);
    }

    [Theory]
    [InlineData(15, 15, true)]
    [InlineData(10, 10, true)]
    [InlineData(20, 20, true)]
    [InlineData(15, 16, true)]
    [InlineData(0, 0, false)]
    public void Contains_ReturnsCorrectResult(float x, float y, bool expected)
    {
        // Given
        var line = new Line(new SKPoint(10, 10), new SKPoint(20, 20));

        // When
        bool result = line.Contains(new SKPoint(x, y));

        // Then
        Assert.Equal(expected, result);
    }
}
