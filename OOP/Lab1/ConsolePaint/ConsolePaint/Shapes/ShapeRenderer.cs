using SkiaSharp;

namespace ConsolePaint.Shapes;

internal sealed class ShapeRenderer
{
    private const float SelectionPadding = 5f;

    public static void Render(SKCanvas? canvas, Shape? shape)
    {
        if (shape == null || canvas == null) return;

        DrawFill(canvas, shape);
        DrawBorder(canvas, shape);

        if (shape.IsSelected)
        {
            DrawSelection(canvas, shape);
        }
    }

    private static void DrawFill(SKCanvas? canvas, Shape? shape)
    {
        using var paint = new SKPaint();

        if (shape == null) return;

        paint.Color = shape.Background;
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;
        canvas?.DrawPath(shape.GetPath(), paint);
    }

    private static void DrawBorder(SKCanvas? canvas, Shape? shape) {
        if (shape == null) return;

        using var paint = new SKPaint();

        paint.Color = shape.BorderColor;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = shape.BorderWidth;
        paint.IsAntialias = true;
        canvas?.DrawPath(shape.GetPath(), paint);
    }

    private static void DrawSelection(SKCanvas? canvas, Shape? shape)
    {
        using var paint = new SKPaint();

        paint.Color = SKColors.Blue;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 2f;
        paint.PathEffect = SKPathEffect.CreateDash([5f, 5f], 0);
        paint.IsAntialias = true;

        if (shape == null) return;

        var bounds = shape.GetBounds();
        canvas?.DrawRect(
            bounds.Left - SelectionPadding,
            bounds.Top - SelectionPadding,
            bounds.Width + 2*SelectionPadding,
            bounds.Height + 2*SelectionPadding,
            paint);
    }
}
