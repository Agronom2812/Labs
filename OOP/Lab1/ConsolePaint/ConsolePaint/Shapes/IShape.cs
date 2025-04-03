using SkiaSharp;

namespace ConsolePaint.Shapes;

public interface IShape
{
    SKColor Background { get; set; }
    SKColor BorderColor { get; set; }
    float BorderWidth { get; set; }
    bool IsSelected { get; set; }
    SKPoint Center { get; set; }

    SKRect GetBounds();
    SKPath GetPath();
    bool Contains(SKPoint point);

    void Move(float dx, float dy);
}
