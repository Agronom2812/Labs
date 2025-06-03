using SkiaSharp;

namespace ConsolePaint.Shapes;

public static class GeometryHelper
{
    public static bool IsPointInTriangle(SKPoint p, SKPoint p1, SKPoint p2, SKPoint p3)
    {
        float d1 = (p.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p.Y - p3.Y);
        float d2 = (p1.X - p3.X) * (p.Y - p3.Y) - (p.X - p3.X) * (p1.Y - p3.Y);
        float d3 = (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    public static bool IsPointNearLine(SKPoint p, SKPoint a, SKPoint b, float tolerance)
    {
        float lineLength = SKPoint.Distance(a, b);
        float d1 = SKPoint.Distance(p, a);
        float d2 = SKPoint.Distance(p, b);
        return Math.Abs(d1 + d2 - lineLength) < tolerance;
    }
}
