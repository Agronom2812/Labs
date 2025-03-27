using System;
using SkiaSharp;

namespace ConsolePaint.Shapes;

public class Triangle : Shape
    {
        private SKPoint[] Vertices { get; set; }
        private float SideA { get; }
        private float SideB { get; }
        private float SideC { get; }

        public Triangle(SKPoint center, float sideA, float sideB, float sideC)
            : base(center)
        {
            if (sideA <= 0 || sideB <= 0 || sideC <= 0)
                throw new ArgumentException("All sides must be positive numbers");

            if (!IsValidTriangle(sideA, sideB, sideC))
                throw new ArgumentException("Invalid triangle sides");

            CalculateVertices(sideA, sideB, sideC);
        }

        private static bool IsValidTriangle(float a, float b, float c)
        {
            return a + b > c && a + c > b && b + c > a;
        }

        public override void Draw(SKCanvas canvas) {
            // Создаем путь для треугольника
            using var path = new SKPath();
            path.MoveTo(Vertices[0]);
            path.LineTo(Vertices[1]);
            path.LineTo(Vertices[2]);
            path.Close();

            // Заливка
            using (var fillPaint = new SKPaint()) {
                fillPaint.Color = Background;
                fillPaint.Style = SKPaintStyle.Fill;
                canvas.DrawPath(path, fillPaint);
            }

            // Обводка
            using (var borderPaint = new SKPaint()) {
                borderPaint.Color = BorderColor;
                borderPaint.Style = SKPaintStyle.Stroke;
                borderPaint.StrokeWidth = BorderWidth;
                canvas.DrawPath(path, borderPaint);
            }
        }

        public override bool Contains(SKPoint point)
        {
            // Алгоритм барицентрических координат
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

        private void CalculateVertices(float a, float b, float c)
        {
            // Расчет вершин (аналогично вашему исходному коду)
            const float x1 = 0, y1 = 0;
            const float y2 = 0;
            float x2 = a;

            double angle = Math.Acos((a * a + b * b - c * c) / (2 * a * b));
            float x3 = (float)(b * Math.Cos(angle));
            float y3 = (float)(b * Math.Sin(angle));

            // Центрирование
            float centerX = (x1 + x2 + x3) / 3;
            float centerY = (y1 + y2 + y3) / 3;

            Vertices = [
                new SKPoint(x1 - centerX + Center.X, y1 - centerY + Center.Y),
                new SKPoint(x2 - centerX + Center.X, y2 - centerY + Center.Y),
                new SKPoint(x3 - centerX + Center.X, y3 - centerY + Center.Y)
            ];
        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = new SKPoint(Vertices[i].X + dx, Vertices[i].Y + dy);
            }
        }
    }
