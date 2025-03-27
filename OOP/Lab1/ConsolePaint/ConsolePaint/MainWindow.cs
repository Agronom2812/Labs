using Gtk;
using ConsolePaint.Shapes;
using ConsolePaint.Commands;
using ConsolePaint.Dialogs;
using SkiaSharp;
using Rectangle = ConsolePaint.Shapes.Rectangle;

namespace ConsolePaint;

public class MainWindow : Window
    {
        private readonly DrawingArea _canvas;
        private readonly CommandManager _cmdManager = new();
        private readonly List<IShape> _shapes = new();
        private SKPoint _lastPointerPos;
        private DrawingTool _currentTool = DrawingTool.Selector;
        private SKSurface? _surface;
        private IShape? _draggedShape;

        [Obsolete("Obsolete")]
        public MainWindow() : base("Paint Application")
        {
            // Window setup
            SetDefaultSize(800, 600);
            DeleteEvent += (_, __) => Application.Quit();

            // Create toolbar
            var toolbar = new Toolbar();
            AddToolButton(toolbar, DrawingTool.Selector, "Select");
            AddToolButton(toolbar, DrawingTool.Line, "Line");
            AddToolButton(toolbar, DrawingTool.Triangle, "Triangle");
            AddToolButton(toolbar, DrawingTool.Rectangle, "Rectangle");
            AddToolButton(toolbar, DrawingTool.Circle, "Circle");
            AddToolButton(toolbar, DrawingTool.Eraser, "Eraser");

            // Create canvas
            _canvas = new DrawingArea();
            _canvas.Drawn += OnCanvasDrawn;
            _canvas.SizeAllocated += OnCanvasResized;

            // Event configuration
            _canvas.Events =
                Gdk.EventMask.ButtonPressMask |
                Gdk.EventMask.ButtonReleaseMask |
                Gdk.EventMask.PointerMotionMask;

            _canvas.ButtonPressEvent += OnPointerDown;
            _canvas.ButtonReleaseEvent += OnPointerUp;
            _canvas.MotionNotifyEvent += OnPointerMove;

            // Layout
            var vbox = new Box(Orientation.Vertical, 0);
            vbox.PackStart(toolbar, false, false, 0);
            vbox.PackEnd(_canvas, true, true, 0);
            Add(vbox);
        }

        [Obsolete("Obsolete")]
        private void AddToolButton(Toolbar toolbar, DrawingTool tool, string label)
        {
            var btn = new ToolButton(label);
            btn.Clicked += (_, __) => _currentTool = tool;
            toolbar.Add(btn);
        }

        private void OnCanvasResized(object sender, SizeAllocatedArgs args)
        {
            // Recreate surface on resize
            _surface?.Dispose();
            _surface = SKSurface.Create(
                new SKImageInfo(
                    args.Allocation.Width,
                    args.Allocation.Height,
                    SKImageInfo.PlatformColorType,
                    SKAlphaType.Premul
                )
            );
            RedrawCanvas();
        }

        private void OnCanvasDrawn(object sender, DrawnArgs args)
        {
            if (_surface == null) return;

            using var ctx = args.Cr;

            // Manual bitmap conversion
            var imageInfo = _surface.PeekPixels().Info;
            var pixBuf = new Gdk.Pixbuf(
                Gdk.Colorspace.Rgb,
                true,
                8,
                imageInfo.Width,
                imageInfo.Height
            );

            _surface.ReadPixels(imageInfo, pixBuf.Pixels, imageInfo.RowBytes, 0, 0);
            Gdk.CairoHelper.SetSourcePixbuf(ctx, pixBuf, 0, 0);
            ctx.Paint();
        }

        private void RedrawCanvas()
        {
            if (_surface == null) return;

            var canvas = _surface.Canvas;
            canvas.Clear(SKColors.White);

            foreach (var shape in _shapes)
            {
                shape.Draw(canvas);
            }

            _canvas.QueueDraw();
        }

        private void OnPointerDown(object o, ButtonPressEventArgs args)
        {
            var point = new SKPoint((float)args.Event.X, (float)args.Event.Y);
            _lastPointerPos = point;

            switch (_currentTool)
            {
                case DrawingTool.Triangle:
                    ShowTriangleDialog(point);
                    break;

                case DrawingTool.Line:
                    _draggedShape = new Line(point, point);
                    AddShape(_draggedShape);
                    break;

                case DrawingTool.Rectangle:
                    _draggedShape = new Rectangle(point, 0, 0);
                    AddShape(_draggedShape);
                    break;

                case DrawingTool.Circle:
                    _draggedShape = new Circle(point, 0);
                    AddShape(_draggedShape);
                    break;

                case DrawingTool.Eraser:
                    RemoveShapeAt(point);
                    break;

                case DrawingTool.Selector:
                    _draggedShape = _shapes.LastOrDefault(s => s.Contains(point));
                    break;
            }
        }

        private void ShowTriangleDialog(SKPoint center)
        {
            using var dialog = new TriangleDialog(this);
            if (dialog.Run() == (int)ResponseType.Ok)
            {
                var triangle = new Triangle(
                    center: center,
                    firstSide: dialog.FirstSide,
                    secondSide: dialog.SecondSide,
                    thirdSide: dialog.ThirdSide,
                    vertices: null // Will be calculated in constructor
                );
                AddShape(triangle);
            }
            dialog.Destroy();
        }

        private void AddShape(IShape shape)
        {
            _cmdManager.Execute(new DrawCommand(_shapes, shape));
            RedrawCanvas();
        }

        private void RemoveShapeAt(SKPoint point)
        {
            var shape = _shapes.LastOrDefault(s => s.Contains(point));
            if (shape != null)
            {
                _cmdManager.Execute(new EraseCommand(_shapes, shape));
                RedrawCanvas();
            }
        }

        private void OnPointerMove(object o, MotionNotifyEventArgs args)
        {
            var point = new SKPoint((float)args.Event.X, (float)args.Event.Y);
            float dx = point.X - _lastPointerPos.X;
            float dy = point.Y - _lastPointerPos.Y;

            if (_draggedShape != null)
            {
                if (_currentTool == DrawingTool.Selector)
                {
                    _draggedShape.Move(dx, dy);
                }
                else switch (_draggedShape) {
                    case Line line:
                        line.EndPoint = point;
                        break;
                    case Rectangle rect:
                        rect.Width = point.X - _lastPointerPos.X;
                        rect.Height = point.Y - _lastPointerPos.Y;
                        break;
                    case Circle circle:
                        circle.Radius = SKPoint.Distance(_lastPointerPos, point);
                        break;
                }

                RedrawCanvas();
            }

            _lastPointerPos = point;
        }

        private void OnPointerUp(object o, ButtonReleaseEventArgs args)
        {
            // Commit the current operation to command history
            if (_draggedShape != null && _currentTool != DrawingTool.Selector)
            {
                _cmdManager.Execute(new DrawCommand(_shapes, _draggedShape));
            }
            _draggedShape = null;
        }
    }

    public enum DrawingTool
    {
        Selector,
        Line,
        Triangle,
        Rectangle,
        Circle,
        Eraser
    }
