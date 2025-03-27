using SkiaSharp;

namespace ConsolePaint.Shapes
{
    public class Circle : Shape
    {
        private float Radius { get; set; }

        public Circle(SKPoint center, float radius) : base(center)
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be a positive number");

            Radius = radius;
        }

        public override void Draw(SKCanvas canvas)
        {
            using (var fillPaint = new SKPaint()) {
                fillPaint.Color = Background;
                fillPaint.Style = SKPaintStyle.Fill;
                canvas.DrawCircle(Center, Radius, fillPaint);
            }

            using (var borderPaint = new SKPaint()) {
                borderPaint.Color = BorderColor;
                borderPaint.Style = SKPaintStyle.Stroke;
                borderPaint.StrokeWidth = BorderWidth;
                canvas.DrawCircle(Center, Radius, borderPaint);
            }
        }

        public override bool Contains(SKPoint point)
        {
            return Math.Pow(point.X - Center.X, 2) + Math.Pow(point.Y - Center.Y, 2) <= Math.Pow(Radius, 2);
        }
    }
}
