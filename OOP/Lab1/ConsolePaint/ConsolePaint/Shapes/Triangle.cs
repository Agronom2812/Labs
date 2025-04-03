using SkiaSharp;

namespace ConsolePaint.Shapes;

public sealed class Triangle : Shape
{
    public SKPoint[] Vertices { get; private set; }
    public float SideA { get; }
    public float SideB { get; }
    public float SideC { get; }

    public Triangle(SKPoint center, float sideA, float sideB, float sideC, SKPoint[]? vertices = null)
    {
        if (sideA <= 0 || sideB <= 0 || sideC <= 0)
            throw new ArgumentException("All sides must be positive");

        if (!IsValidTriangle(sideA, sideB, sideC))
            throw new ArgumentException("Invalid triangle sides");

        SideA = sideA;
        SideB = sideB;
        SideC = sideC;
        Center = center;
        Vertices = vertices ?? CalculateVertices(center, sideA, sideB, sideC);
    }

    public override SKRect GetBounds()
    {
        float minX = Vertices.Min(v => v.X);
        float minY = Vertices.Min(v => v.Y);
        float maxX = Vertices.Max(v => v.X);
        float maxY = Vertices.Max(v => v.Y);
        return SKRect.Create(minX, minY, maxX - minX, maxY - minY);
    }

    public override SKPath GetPath()
    {
        var path = new SKPath();
        path.MoveTo(Vertices[0]);
        path.LineTo(Vertices[1]);
        path.LineTo(Vertices[2]);
        path.Close();
        return path;
    }

    public override bool Contains(SKPoint point)
    {
        return GeometryHelper.IsPointInTriangle(point, Vertices[0], Vertices[1], Vertices[2]);
    }

    public override void Move(float dx, float dy)
    {
        base.Move(dx, dy);
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i] = new SKPoint(Vertices[i].X + dx, Vertices[i].Y + dy);
        }
    }

    public static SKPoint[] CalculateVertices(SKPoint center, float a, float b, float c)
    {
        var p2 = new SKPoint(center.X + a, center.Y);

        float angle = (float)Math.Acos((a*a + b*b - c*c) / (2*a*b));
        var p3 = new SKPoint(
            center.X + b * (float)Math.Cos(angle),
            center.Y + b * (float)Math.Sin(angle));

        return [center, p2, p3];
    }

    public static bool IsValidTriangle(float a, float b, float c)
    {
        return a + b > c && a + c > b && b + c > a;
    }
}
