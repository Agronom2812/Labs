﻿using SkiaSharp;

namespace ConsolePaint.Shapes;

public abstract class Shape() : IShape
{
    public SKPoint Center { get; set; }
    public SKColor Background { get; set; } = SKColors.White;
    public SKColor BorderColor { get; set; } = SKColors.Black;
    public float BorderWidth { get; set; } = 1;

    public bool IsSelected { get; set; }

    public abstract void Draw(SKCanvas canvas);
    public abstract bool Contains(SKPoint point);

    public virtual void Move(float dx, float dy)
    {
        Center = new SKPoint(Center.X + dx, Center.Y + dy);
    }
}
