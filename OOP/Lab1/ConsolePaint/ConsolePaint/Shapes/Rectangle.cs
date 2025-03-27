using SkiaSharp;

namespace ConsolePaint.Shapes;

public class Rectangle : Shape
{
    private float Width { get; set; }
    private float Height { get; set; }

    public Rectangle(SKPoint center, float width, float height) : base(center)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and height must be positive numbers");

        Width = width;
        Height = height;
    }

    public override void Draw(SKCanvas canvas)
    {
        var rect = new SKRect(
            Center.X - Width / 2,
            Center.Y - Height / 2,
            Center.X + Width / 2,
            Center.Y + Height / 2);

        using (var fillPaint = new SKPaint()) {
            fillPaint.Color = Background;
            fillPaint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(rect, fillPaint);
        }

        using (var borderPaint = new SKPaint()) {
            borderPaint.Color = BorderColor;
            borderPaint.Style = SKPaintStyle.Stroke;
            borderPaint.StrokeWidth = BorderWidth;
            canvas.DrawRect(rect, borderPaint);
        }
    }

    public override bool Contains(SKPoint point)
    {
        float halfWidth = Width / 2;
        float halfHeight = Height / 2;

        return point.X >= Center.X - halfWidth &&
               point.X <= Center.X + halfWidth &&
               point.Y >= Center.Y - halfHeight &&
               point.Y <= Center.Y + halfHeight;
    }
}
