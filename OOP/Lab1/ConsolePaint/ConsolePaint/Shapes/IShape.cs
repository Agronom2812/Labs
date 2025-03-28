using SkiaSharp;

namespace ConsolePaint.Shapes;

public interface IShape
{
    void Draw(SKCanvas canvas);
    bool Contains(SKPoint point);
    void Move(float dx, float dy);
    SKColor Background { get; set; }
    public bool IsSelected { get; set; }
    SKColor BorderColor { get; set; }
    float BorderWidth { get; set; }
    SKPoint Center { get; set; }
}
