using SkiaSharp;

namespace ConsolePaint.Shapes
{
    /// <summary>
    /// Represents a circle shape with specified radius.
    /// </summary>
    public sealed class Circle : Shape
    {
        /// <summary>
        /// Gets or sets the radius of the circle.
        /// </summary>
        private float Radius { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circle"/> class.
        /// </summary>
        /// <param name="center">The center point of the circle.</param>
        /// <param name="radius">The radius of the circle (must be positive).</param>
        /// <exception cref="ArgumentException">
        /// Thrown when radius is non-positive.
        /// </exception>
        public Circle(SKPoint center, float radius) : base()
        {
            if (radius <= 0)
                throw new ArgumentException("Radius must be a positive number");

            Radius = radius;
        }

        /// <summary>
        /// Draws the circle on the specified canvas.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to draw on.</param>
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

        /// <summary>
        /// Checks if a point is contained within the circle.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>
        /// <c>true</c> if the point is inside the circle.
        /// <c>false</c> if the point is not inside the circle.
        /// </returns>
        public override bool Contains(SKPoint point)
        {
            return Math.Pow(point.X - Center.X, 2) + Math.Pow(point.Y - Center.Y, 2) <= Math.Pow(Radius, 2);
        }
    }
}
