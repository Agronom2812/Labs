using SkiaSharp;

namespace ConsolePaint.Shapes;

public class Line : Shape
{
    public SKPoint EndPoint { get; set; }

    public Line(SKPoint start, SKPoint end)
    {
        EndPoint = end;
        BorderWidth = 2; // Более толстые линии по умолчанию
    }

    public override void Draw(SKCanvas canvas)
    {
        using var paint = new SKPaint();
        paint.Color = BorderColor;
        paint.StrokeWidth = BorderWidth;
        paint.Style = SKPaintStyle.Stroke;
        canvas.DrawLine(Center, EndPoint, paint);
    }

    public override bool Contains(SKPoint point)
    {
        // Проверка расстояния до линии
        return DistanceToLine(point, Center, EndPoint) <= BorderWidth * 2;
    }

    private static float DistanceToLine(SKPoint p, SKPoint a, SKPoint b)
    {
        // Вектор AB
        SKPoint ab = new SKPoint(b.X - a.X, b.Y - a.Y);
        // Вектор AP
        SKPoint ap = new SKPoint(p.X - a.X, p.Y - a.Y);

        float abLengthSquared = ab.X * ab.X + ab.Y * ab.Y;
        float dot = ap.X * ab.X + ap.Y * ab.Y;
        float t = Math.Clamp(dot / abLengthSquared, 0, 1);

        // Проекция точки на линию
        SKPoint projection = new SKPoint(a.X + t * ab.X, a.Y + t * ab.Y);

        return SKPoint.Distance(p, projection);
    }
}
