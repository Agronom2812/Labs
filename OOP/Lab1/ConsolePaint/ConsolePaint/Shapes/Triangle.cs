using System;
using SkiaSharp;

namespace ConsolePaint.Shapes;

/// <summary>
/// Represents a triangle shape.
/// </summary>
/// <remarks>
/// The triangle is constructed using the specified side lengths and automatically calculates vertex positions while
/// maintaining the given center point.
/// </remarks>
public class Triangle : Shape
    {
        /// <summary>
        /// Gets the vertices of the triangle in screen coordinates.
        /// </summary>
        private SKPoint[] Vertices { get; set; }

        /// <summary>
        /// Gets the length of the first side of the triangle.
        /// </summary>
        private float firstSide { get; }

        /// <summary>
        /// Gets the length of the second side of the triangle.
        /// </summary>
        private float secondSide { get; }

        /// <summary>
        /// Gets the length of the third side of the triangle.
        /// </summary>
        private float thirdSide { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class.
        /// </summary>
        /// <param name="center">The center point of the triangle.</param>
        /// <param name="firstSide">Length of the first side of the triangle (must be positive).</param>
        /// <param name="secondSide">Length of the second side of the triangle (must be positive).</param>
        /// <param name="thirdSide">Length of the third side of the triangle (must be positive).</param>
        /// <exception cref="ArgumentException">
        /// Thrown when sides are non-positive or don't satisfy triangle inequality.
        /// </exception>
        public Triangle(SKPoint center, float firstSide, float secondSide, float thirdSide)
            : base(center)
        {
            if (firstSide <= 0 || secondSide <= 0 || thirdSide <= 0)
                throw new ArgumentException("All sides must be positive numbers");

            if (!IsValidTriangle(firstSide, secondSide, thirdSide))
                throw new ArgumentException("Invalid triangle sides");

            CalculateVertices(firstSide, secondSide, thirdSide);
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
        public override void Draw(SKCanvas canvas) {

            using var path = new SKPath();
            path.MoveTo(Vertices[0]);
            path.LineTo(Vertices[1]);
            path.LineTo(Vertices[2]);
            path.Close();

            using (var fillPaint = new SKPaint()) {
                fillPaint.Color = Background;
                fillPaint.Style = SKPaintStyle.Fill;
                canvas.DrawPath(path, fillPaint);
            }

            using (var borderPaint = new SKPaint()) {
                borderPaint.Color = BorderColor;
                borderPaint.Style = SKPaintStyle.Stroke;
                borderPaint.StrokeWidth = BorderWidth;
                canvas.DrawPath(path, borderPaint);
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
        /// <param name="firstSide">Length of the first side of the triangle.</param>
        /// <param name="secondSide">Length of the second side of the triangle.</param>
        /// <param name="thirdSide">Length of the third side of the triangle.</param>
        /// <remarks>
        /// The method:
        /// <list type="bullet">
        /// <item>Starts with vertex A at (0,0) and vertex B at (sideA,0)</item>
        /// <item>Calculates vertex C using the law of cosines</item>
        /// <item>Centers the triangle around (0,0) and then translates it to the specified center</item>
        /// </list>
        /// </remarks>
        private void CalculateVertices(float firstSide, float secondSide, float thirdSide)
        {
            const float x1 = 0, y1 = 0;
            const float y2 = 0;
            float x2 = firstSide;

            double angle = Math.Acos((firstSide * firstSide + secondSide * secondSide - thirdSide * thirdSide)
                                     / (2 * firstSide * secondSide));
            float x3 = (float)(secondSide * Math.Cos(angle));
            float y3 = (float)(secondSide * Math.Sin(angle));

            float centerX = (float)((x1 + x2 + x3) / 3.0);
            float centerY = (float)((y1 + y2 + y3) / 3.0);

            Vertices = [
                new SKPoint(x1 - centerX + Center.X, y1 - centerY + Center.Y),
                new SKPoint(x2 - centerX + Center.X, y2 - centerY + Center.Y),
                new SKPoint(x3 - centerX + Center.X, y3 - centerY + Center.Y)
            ];
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
        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = new SKPoint(Vertices[i].X + dx, Vertices[i].Y + dy);
            }
        }
    }
