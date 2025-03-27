using SkiaSharp;

namespace ConsolePaint.Shapes;

/// <summary>
/// Represents a rectangle shape with specified width and height.
/// </summary>
public sealed class Rectangle : Shape
{
    /// <summary>
    /// Gets or sets the width of the rectangle.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the rectangle.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rectangle"/> class.
    /// </summary>
    /// <param name="center">The center point of the rectangle.</param>
    /// <param name="width">The width of the rectangle (must be positive).</param>
    /// <param name="height">The height of the rectangle (must be positive).</param>
    /// <exception cref="ArgumentException">
    /// Thrown when width or height are non-positive.
    /// </exception>
    public Rectangle(SKPoint center, float width, float height) : base()
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and height must be positive numbers");

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Draws the rectangle on the specified canvas.
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on.</param>
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

    /// <summary>
    /// Checks if a point is contained within the rectangle.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>
    /// <c>true</c> if the point is inside the rectangle.
    /// <c>false</c> if the point is not inside the triangle.
    /// </returns>
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
