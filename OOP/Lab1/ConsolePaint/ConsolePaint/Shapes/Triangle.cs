using SkiaSharp;

namespace ConsolePaint.Shapes;

/// <summary>
/// Represents a triangle shape.
/// </summary>
/// <remarks>
/// The triangle is constructed using the specified side lengths and automatically calculates vertex positions while
/// maintaining the given center point.
/// </remarks>
public sealed class Triangle : Shape
    {
        /// <summary>
        /// Gets the vertices of the triangle in screen coordinates.
        /// </summary>
        public SKPoint[]? Vertices { get; set; }

        /// <summary>
        /// Gets the length of the first side of the triangle.
        /// </summary>
        public float FirstSide { get; }

        /// <summary>
        /// Gets the length of the second side of the triangle.
        /// </summary>
        public float SecondSide { get; }

        /// <summary>
        /// Gets the length of the third side of the triangle.
        /// </summary>
        public float ThirdSide { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class.
        /// </summary>
        /// <param name="center">The center point of the triangle.</param>
        /// <param name="firstSide">Length of the first side of the triangle (must be positive).</param>
        /// <param name="secondSide">Length of the second side of the triangle (must be positive).</param>
        /// <param name="thirdSide">Length of the third side of the triangle (must be positive).</param>
        /// <param name="vertices">The vertices of the triangle</param>
        /// <exception cref="ArgumentException">
        /// Thrown when sides are non-positive or don't satisfy triangle inequality.
        /// </exception>
        public Triangle(SKPoint center, float firstSide, float secondSide, float thirdSide, SKPoint[]? vertices)
        {
            if (firstSide <= 0 || secondSide <= 0 || thirdSide <= 0)
                throw new ArgumentException("All sides must be positive numbers");

            if (!IsValidTriangle(firstSide, secondSide, thirdSide))
                throw new ArgumentException("Invalid triangle sides");

            FirstSide = firstSide;
            SecondSide = secondSide;
            ThirdSide = thirdSide;
            Vertices = vertices;

            Vertices = vertices ?? CalculateVertices(center, firstSide, secondSide, thirdSide);

        }

        /// <summary>
        /// Checks whether a triangle with the given sides can exist.
        /// </summary>
        /// <param name="firstSide">Length of the first side of the triangle.</param>
        /// <param name="secondSide">Length of the second side of the triangle.</param>
        /// <param name="thirdSide">Length of the third side of the triangle.</param>
        /// <returns>
        /// <c>true</c> if triangle can exist.
        /// <c>false</c> if triangle can not exist.
        /// </returns>
        private static bool IsValidTriangle(float firstSide, float secondSide, float thirdSide)
        {
            return firstSide + secondSide > thirdSide && firstSide + thirdSide > secondSide
                                                      && secondSide + thirdSide > firstSide;
        }

        /// <summary>
        /// Draws the triangle on the specified canvas.
        /// </summary>
        /// <param name="canvas">The <c>SkiaSharp</c> canvas to draw on.</param>
        public override void Draw(SKCanvas canvas)
        {
            if (Vertices == null || Vertices.Length != 3) return;

            using var path = new SKPath();
            path.MoveTo(Vertices[0]);
            path.LineTo(Vertices[1]);
            path.LineTo(Vertices[2]);
            path.Close();

            using (var fillPaint = new SKPaint())
            {
                fillPaint.Color = Background;
                fillPaint.Style = SKPaintStyle.Fill;
                canvas.DrawPath(path, fillPaint);
            }

            using (var borderPaint = new SKPaint())
            {
                borderPaint.Color = BorderColor;
                borderPaint.Style = SKPaintStyle.Stroke;
                borderPaint.StrokeWidth = BorderWidth;
                canvas.DrawPath(path, borderPaint);
            }

            if (IsSelected)
            {
                using var selectionPaint = new SKPaint
                {
                    Color = SKColors.Blue,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0),
                    IsAntialias = true
                };

                float minX = Vertices.Min(v => v.X);
                float minY = Vertices.Min(v => v.Y);
                float maxX = Vertices.Max(v => v.X);
                float maxY = Vertices.Max(v => v.Y);

                var selectionRect = new SKRect(
                    minX - 5,
                    minY - 5,
                    maxX + 5,
                    maxY + 5);

                canvas.DrawRect(selectionRect, selectionPaint);
            }
        }

        /// <summary>
        /// Checks if a point is contained within the triangle.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>
        /// <c>true</c> if the point is inside the triangle.
        /// <c>false</c> if the point is not inside the triangle.
        /// </returns>
        public override bool Contains(SKPoint point)
        {
            var p1 = Vertices[0];
            var p2 = Vertices[1];
            var p3 = Vertices[2];

            float d1 = (point.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (point.Y - p3.Y);
            float d2 = (p1.X - p3.X) * (point.Y - p3.Y) - (point.X - p3.X) * (p1.Y - p3.Y);
            float d3 = (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        /// <summary>
        /// Calculates the vertices of the triangle based on side lengths and current center position.
        /// </summary>
        /// <param name="a">Length of the first side of the triangle.</param>
        /// <param name="b">Length of the second side of the triangle.</param>
        /// <param name="c">Length of the third side of the triangle.</param>
        /// <param name="center"></param>
        /// <remarks>
        /// The method:
        /// <list type="bullet">
        /// <item>Starts with vertex A at (0,0) and vertex B at (sideA,0)</item>
        /// <item>Calculates vertex C using the law of cosines</item>
        /// <item>Centers the triangle around (0,0) and then translates it to the specified center</item>
        /// </list>
        /// </remarks>
        private static SKPoint[] CalculateVertices(SKPoint center, float a, float b, float c)
        {
            var p1 = center;

            var p2 = new SKPoint(center.X + a, center.Y);

            float angle = (float)Math.Acos((a*a + b*b - c*c) / (2*a*b));
            float px = center.X + b * (float)Math.Cos(angle);
            float py = center.Y + b * (float)Math.Sin(angle);
            var p3 = new SKPoint(px, py);

            return [p1, p2, p3];
        }

        /// <summary>
        /// Moves the triangle by specified offsets in both X and Y directions.
        /// </summary>
        /// <param name="dx">Horizontal offset (positive = right, negative = left).</param>
        /// <param name="dy">Vertical offset (positive = down, negative = up).</param>
        /// <remarks>
        /// This method:
        /// <list type="bullet">
        /// <item>Updates the <see cref="Shape.Center"/> position by adding the offsets</item>
        /// <item>Recalculates all vertices to maintain triangle geometry</item>
        /// <item>Preserves the exact side lengths during movement</item>
        /// </list>
        /// </remarks>
        public override void Move(float dx, float dy) {
            base.Move(dx, dy);
            if (Vertices == null) return;

            for (int i = 0; i < Vertices.Length; i++) {
                Vertices[i] = new SKPoint(Vertices[i].X + dx, Vertices[i].Y + dy);
            }
        }
    }
