using SkiaSharp;

namespace ConsolePaint.Shapes;

public sealed class Rectangle : Shape
{
    public float Width { get; set; }
    public float Height { get; set; }

    public Rectangle(SKPoint center, float width, float height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and height must be positive");

        Center = center;
        Width = width;
        Height = height;
    }

    public override SKRect GetBounds()
    {
        return SKRect.Create(
            Center.X - Width/2,
            Center.Y - Height/2,
            Width,
            Height);
    }

    public override SKPath GetPath()
    {
        var path = new SKPath();
        path.AddRect(GetBounds());
        return path;
    }

    public override bool Contains(SKPoint point)
    {
        var bounds = GetBounds();
        return bounds.Contains(point.X, point.Y);
    }
}
