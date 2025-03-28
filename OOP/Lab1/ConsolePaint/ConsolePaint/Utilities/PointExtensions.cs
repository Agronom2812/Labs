using SkiaSharp;

namespace ConsolePaint.Utilities;

public static class PointExtensions
{
    public static SKPoint ToSkPoint(this (float x, float y) tuple)
    {
        return new SKPoint(tuple.x, tuple.y);
    }

    public static float DistanceTo(this SKPoint point1, SKPoint point2)
    {
        return SKPoint.Distance(point1, point2);
    }

    public static SKPoint Offset(this SKPoint point, float dx, float dy)
    {
        return new SKPoint(point.X + dx, point.Y + dy);
    }
}
