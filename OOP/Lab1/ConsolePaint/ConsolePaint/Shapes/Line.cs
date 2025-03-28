using System.Text.Json.Serialization;
using SkiaSharp;

namespace ConsolePaint.Shapes;

public sealed class Line : Shape {

    [JsonPropertyName("StartPoint")]
    public SKPoint StartPoint { get; set; }

    [JsonPropertyName("EndPoint")]
    public SKPoint EndPoint { get; set; }


    public Line(SKPoint start, SKPoint end) : base() {
        StartPoint = start;
        EndPoint = end;
        UpdateCenter();

        BorderColor = SKColors.Black;
        BorderWidth = 2;
    }

    private void UpdateCenter() {
        Center = new SKPoint(
            (StartPoint.X + EndPoint.X) / 2,
            (StartPoint.Y + EndPoint.Y) / 2
        );
    }

    public override void Draw(SKCanvas canvas) {
        using (var strokePaint = new SKPaint()) {
            strokePaint.Color = BorderColor;
            strokePaint.Style = SKPaintStyle.Stroke;
            strokePaint.StrokeWidth = BorderWidth;
            strokePaint.IsAntialias = true;
            canvas.DrawLine(StartPoint, EndPoint, strokePaint);
        }

        if (IsSelected) {
            using var selectionPaint = new SKPaint {
                Color = SKColors.Blue,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                PathEffect = SKPathEffect.CreateDash([5, 5], 0),
                IsAntialias = true
            };

            var bounds = new SKRect(
                Math.Min(StartPoint.X, EndPoint.X) - 5,
                Math.Min(StartPoint.Y, EndPoint.Y) - 5,
                Math.Max(StartPoint.X, EndPoint.X) + 5,
                Math.Max(StartPoint.Y, EndPoint.Y) + 5);

            canvas.DrawRect(bounds, selectionPaint);
        }
    }

    public override bool Contains(SKPoint point) {

        float lineLength = Distance(StartPoint, EndPoint);
        float d1 = Distance(point, StartPoint);
        float d2 = Distance(point, EndPoint);

        return Math.Abs(d1 + d2 - lineLength) < 5;
    }

    private float Distance(SKPoint a, SKPoint b) {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public override void Move(float dx, float dy) {
        StartPoint = new SKPoint(StartPoint.X + dx, StartPoint.Y + dy);
        EndPoint = new SKPoint(EndPoint.X + dx, EndPoint.Y + dy);
        UpdateCenter();
    }
}
