using SkiaSharp;

namespace ConsolePaint.Shapes;

public abstract class Shape : IShape
{
    public SKPoint Center { get; set; }
    public SKColor Background { get; set; } = SKColors.Transparent;
    public SKColor BorderColor { get; set; } = SKColors.Black;
    public float BorderWidth { get; set; } = 1f;
    public bool IsSelected { get; set; }

    public abstract bool Contains(SKPoint point);
    public virtual SKRect GetBounds()
    {
        throw new NotImplementedException("Этот метод должен быть переопределен в производных классах");
    }
    public abstract SKPath GetPath();

    public virtual SKPoint StartPoint { get; set; }
    public virtual SKPoint EndPoint { get; set; }

    public virtual void Move(float dx, float dy)
    {
        Center = new SKPoint(Center.X + dx, Center.Y + dy);
    }

    public virtual Dictionary<string, object> GetGeometry()
    {
        return new Dictionary<string, object>
        {
            { "Center", Center },
            { "Type", GetType().Name }
        };
    }
}
