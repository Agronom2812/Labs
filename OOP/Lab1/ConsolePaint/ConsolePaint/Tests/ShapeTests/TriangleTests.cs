using ConsolePaint.Shapes;
using SkiaSharp;
using Xunit;

namespace ConsolePaint.Tests.ShapeTests;

public sealed class TriangleTests
{
    public static TheoryData<SKPoint, float, float, float, SKPoint[]> TriangleData =>
        new()
        {
            {
                new SKPoint(10, 10),
                5, 5, 5,
                [
                    new SKPoint(10, 10),
                    new SKPoint(15, 10),
                    new SKPoint(12.5f, 10 + 4.330127f)
                ]
            }
        };

    [Theory]
    [MemberData(nameof(TriangleData))]
    public void CalculateVertices_ReturnsCorrectPoints(
        SKPoint center,
        float a, float b, float c,
        SKPoint[] expectedVertices)
    {
        // Given
        var vertices = Triangle.CalculateVertices(center, a, b, c);

        // When
        Assert.Equal(expectedVertices.Length, vertices.Length);

        // Then
        for (int i = 0; i < vertices.Length; i++)
        {
            Assert.Equal(expectedVertices[i].X, vertices[i].X, precision: 3);
            Assert.Equal(expectedVertices[i].Y, vertices[i].Y, precision: 3);
        }
    }

    [Theory]
    [InlineData(1, 1, 1, true)]
    [InlineData(1, 2, 3, false)]
    public void IsValidTriangle_ReturnsCorrectResult(float a, float b, float c, bool expected)
    {
        Assert.Equal(expected, Triangle.IsValidTriangle(a, b, c));
    }
}
