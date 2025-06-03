using SkiaSharp;

namespace ConsolePaint.Shapes;

public sealed class Circle : Shape
{
    public float Radius { get; set; }

    public Circle(SKPoint center, float radius)
    {
        if (radius <= 0)
            throw new ArgumentException("Radius must be positive", nameof(radius));

        Center = center;
        Radius = radius;
    }

    public override bool Contains(SKPoint point)
    {
        float dx = point.X - Center.X;
        float dy = point.Y - Center.Y;
        return dx * dx + dy * dy <= Radius * Radius;
    }

    public override SKRect GetBounds()
    {
        return SKRect.Create(
            x: Center.X - Radius,
            y: Center.Y - Radius,
            width: 2 * Radius,
            height: 2 * Radius);
    }

    public override SKPath GetPath()
    {
        var path = new SKPath();
        path.AddCircle(
            x: Center.X,
            y: Center.Y,
            radius: Radius,
            dir: SKPathDirection.Clockwise);
        return path;
    }

    public override Dictionary<string, object> GetGeometry()
    {
        return new Dictionary<string, object>
        {
            ["Type"] = "Circle",
            ["CenterX"] = Center.X,
            ["CenterY"] = Center.Y,
            ["Radius"] = Radius
        };
    }
}
