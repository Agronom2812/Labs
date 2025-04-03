using SkiaSharp;

namespace ConsolePaint.Shapes;

public sealed class Line : Shape
{
    public new SKPoint StartPoint { get; private set; }
    public new SKPoint EndPoint { get; set; }

    public Line(SKPoint start, SKPoint end)
    {
        StartPoint = start;
        EndPoint = end;
        UpdateCenter();
    }

    public override SKRect GetBounds()
    {
        return SKRect.Create(
            Math.Min(StartPoint.X, EndPoint.X),
            Math.Min(StartPoint.Y, EndPoint.Y),
            Math.Abs(StartPoint.X - EndPoint.X),
            Math.Abs(StartPoint.Y - EndPoint.Y));
    }

    public override SKPath GetPath()
    {
        var path = new SKPath();
        path.MoveTo(StartPoint);
        path.LineTo(EndPoint);
        return path;
    }

    public override bool Contains(SKPoint point)
    {
        return GeometryHelper.IsPointNearLine(point, StartPoint, EndPoint, 5f);
    }

    public override void Move(float dx, float dy)
    {
        StartPoint = new SKPoint(StartPoint.X + dx, StartPoint.Y + dy);
        EndPoint = new SKPoint(EndPoint.X + dx, EndPoint.Y + dy);
        UpdateCenter();
    }

    private void UpdateCenter()
    {
        Center = new SKPoint(
            (StartPoint.X + EndPoint.X) / 2,
            (StartPoint.Y + EndPoint.Y) / 2);
    }
}
