﻿using Gtk;
using ConsolePaint.Shapes;
using ConsolePaint.Commands;
using ConsolePaint.Dialogs;
using ConsolePaint.Utilities;
using Gdk;
using SkiaSharp;
using Rectangle = ConsolePaint.Shapes.Rectangle;
using Window = Gtk.Window;

namespace ConsolePaint
{
    public sealed class MainWindow : Window
    {
        private readonly DrawingArea _canvas;
        private readonly CommandManager _cmdManager = new();
        private readonly List<IShape> _shapes;
        private SKPoint _startPoint;
        private DrawingTool _currentTool = DrawingTool.Selector;
        private SKSurface? _surface;
        private IShape? _currentShape;
        private bool _isDrawing;
        private IShape? _selectedShape;
        private SKPoint _selectionOffset;
        private bool _isDragging;
        private readonly Button _undoButton;
        private readonly Button _redoButton;
        private readonly Button _fillColorButton;

        public MainWindow() : base("Console Paint")
        {
            SetDefaultSize(800, 600);
            DeleteEvent += (_, _) => Application.Quit();

            _shapes = [];

            var toolbar = new Box(Orientation.Horizontal, 5)
            {
                Margin = 5,
                Halign = Align.Start
            };

            _undoButton = new Button ("Undo")
            {
                Visible = true,
                Margin = 2,
                Sensitive = false
            };
            _undoButton.Clicked += OnUndoClicked;

            _redoButton = new Button("Redo")
            {
                Visible = true,
                Margin = 2,
                Sensitive = false
            };
            _redoButton.Clicked += OnRedoClicked;

            _fillColorButton = new Button
            {
                Label = "Fill",
                Sensitive = false,
                Visible = true,
                Margin = 5
            };
            _fillColorButton.Clicked += OnFillColorClicked;

            toolbar.PackStart(_fillColorButton, false, false, 0);

            toolbar.PackEnd(_undoButton, false, false, 0);
            toolbar.PackEnd(_redoButton, false, false, 0);

            AddToolButton(toolbar, DrawingTool.Selector, "Select");
            AddToolButton(toolbar, DrawingTool.Line, "Line");
            AddToolButton(toolbar, DrawingTool.Triangle, "Triangle");
            AddToolButton(toolbar, DrawingTool.Rectangle, "Rectangle");
            AddToolButton(toolbar, DrawingTool.Circle, "Circle");
            AddToolButton(toolbar, DrawingTool.Eraser, "Eraser");

            var saveButton = new Button { Label = "Save", Margin = 2 };
            saveButton.Clicked += OnSaveClicked;
            toolbar.PackEnd(saveButton, false, false, 0);

            var loadButton = new Button { Label = "Load", Margin = 2 };
            loadButton.Clicked += OnLoadClicked;
            toolbar.PackEnd(loadButton, false, false, 0);

            _canvas = new DrawingArea();
            _canvas.Drawn += OnCanvasDrawn;
            _canvas.SizeAllocated += OnCanvasResized;

            _canvas.Events =
                EventMask.ButtonPressMask |
                EventMask.ButtonReleaseMask |
                EventMask.PointerMotionMask;

            _canvas.ButtonPressEvent += OnPointerDown;
            _canvas.ButtonReleaseEvent += OnPointerUp;
            _canvas.MotionNotifyEvent += OnPointerMove;

            var clearButton = new Button
            {
                Label = "Clear All",
                Margin = 2,
                Expand = false
            };
            clearButton.Clicked += OnClearClicked;
            toolbar.PackStart(clearButton, false, false, 0);

            var vbox = new Box(Orientation.Vertical, 0);
            vbox.PackStart(toolbar, false, false, 0);
            vbox.PackEnd(_canvas, true, true, 0);
            Add(vbox);
        }

        private void OnUndoClicked(object? sender, EventArgs e)
        {
            _cmdManager.Undo();
            RedrawCanvas();
            UpdateUndoRedoButtons();
        }

        private void OnRedoClicked(object? sender, EventArgs e)
        {
            _cmdManager.Redo();
            RedrawCanvas();
            UpdateUndoRedoButtons();
        }
        private void UpdateUndoRedoButtons()
        {
            _undoButton.Sensitive = _cmdManager.CanUndo();
            _redoButton.Sensitive = _cmdManager.CanRedo();

            Console.WriteLine($"Undo available: {_cmdManager.CanUndo()}, Redo available: {_cmdManager.CanRedo()}");
        }


        private void AddToolButton(Box container, DrawingTool tool, string label)
        {
            var button = new Button
            {
                Label = label,
                Margin = 2,
                Expand = false
            };

            button.Clicked += (_, _) =>
            {
                _currentTool = tool;
                Console.WriteLine($"Tool selected: {tool}");
            };

            container.PackStart(button, false, false, 0);
        }

        private void OnCanvasResized(object sender, SizeAllocatedArgs args)
        {
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

        private void OnCanvasDrawn(object sender, DrawnArgs args) {
            if (_surface == null) return;

            using var ctx = args.Cr;
            var imageInfo = _surface.PeekPixels().Info;
            var pixBuf = new Pixbuf(
                Colorspace.Rgb,
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
            if (_surface == null)
                return;

            try
            {
                using (var canvas = _surface.Canvas)
                {
                    canvas.Clear(SKColors.White);

                    foreach (var shape in _shapes.OfType<Shape>()) {
                        ShapeRenderer.Render(canvas, shape);
                    }

                    if (_currentShape is Shape current)
                    {
                        ShapeRenderer.Render(canvas, current);
                    }
                }

                _canvas.QueueDraw();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при перерисовке: {ex}");
            }
        }

        private void OnPointerDown(object o, ButtonPressEventArgs? args)
{
    try
    {
        if (args?.Event == null)
        {
            Console.WriteLine("Invalid button press event");
            return;
        }

        var point = new SKPoint((float)args.Event.X, (float)args.Event.Y);

        if (_currentTool == DrawingTool.Selector)
        {
            foreach (var shape in _shapes.OfType<IShape>()) {
                shape.IsSelected = false;
            }

            _selectedShape = null;
            for (int i = _shapes.Count - 1; i >= 0; i--)
            {
                var shape = _shapes[i];
                if (shape.Contains(point) != true) continue;

                _selectedShape = shape;
                _selectedShape.IsSelected = true;
                break;
            }

            if (_selectedShape != null)
            {
                if (_selectedShape is Line line)
                {
                    var closestPoint = GetClosestPointOnLine(line.StartPoint, line.EndPoint, point);
                    _selectionOffset = new SKPoint(
                        point.X - closestPoint.X,
                        point.Y - closestPoint.Y);
                }
                else
                {
                    _selectionOffset = new SKPoint(
                        point.X - _selectedShape.Center.X,
                        point.Y - _selectedShape.Center.Y);
                }
                _isDragging = true;
            }

            UpdateSelectionState();
            return;
        }

        _startPoint = point;
        _isDrawing = true;

        switch (_currentTool)
        {
            case DrawingTool.Line:
                _currentShape = new Line(point, point)
                {
                    BorderColor = SKColors.Black,
                    Background = SKColors.Transparent,
                    BorderWidth = 2
                };
                _cmdManager.Execute(new DrawCommand(_shapes, _currentShape));
                break;

            case DrawingTool.Rectangle:
                ShowRectangleDialog(point);
                _isDrawing = false;
                break;

            case DrawingTool.Circle:
                ShowCircleDialog(point);
                _isDrawing = false;
                break;

            case DrawingTool.Triangle:
                ShowTriangleDialog(point);
                _isDrawing = false;
                break;

            case DrawingTool.Eraser:
                RemoveShapeAt(point);
                _isDrawing = false;
                break;
            case DrawingTool.Selector:
                break;
        }

        RedrawCanvas();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in OnPointerDown: {ex}");
        ShowErrorDialog("Ошибка при обработке нажатия");
    }
}

        private void UpdateSelectionState()
        {
            Application.Invoke((_, _) =>
            {
                try
                {
                    bool shouldBeSensitive = _selectedShape != null;
                    Console.WriteLine($"Setting button sensitive to {shouldBeSensitive}");

                    _fillColorButton.Sensitive = shouldBeSensitive;

                    _fillColorButton.QueueDraw();
                    _fillColorButton.Parent?.QueueDraw();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in UI update: {ex}");
                }
            });
        }

        private static SKPoint GetClosestPointOnLine(SKPoint start, SKPoint end, SKPoint point)
        {
            SKPoint lineVector = new SKPoint(end.X - start.X, end.Y - start.Y);

            SKPoint pointVector = new SKPoint(point.X - start.X, point.Y - start.Y);

            float lineLengthSquared = lineVector.X * lineVector.X + lineVector.Y * lineVector.Y;

            float dotProduct = pointVector.X * lineVector.X + pointVector.Y * lineVector.Y;

            float t = dotProduct / lineLengthSquared;
            t = Math.Max(0, Math.Min(1, t));

            return new SKPoint(
                start.X + t * lineVector.X,
                start.Y + t * lineVector.Y
            );
        }


        private void OnPointerMove(object o, MotionNotifyEventArgs args)
        {
            var currentPoint = new SKPoint((float)args.Event.X, (float)args.Event.Y);

            if (_currentTool == DrawingTool.Selector && _isDragging && _selectedShape != null)
            {
                if (_selectedShape is Line line)
                {
                    float dx = currentPoint.X - _selectionOffset.X - line.Center.X;
                    float dy = currentPoint.Y - _selectionOffset.Y - line.Center.Y;
                    line.Move(dx, dy);
                }
                else
                {
                    _selectedShape.Center = new SKPoint(
                        currentPoint.X - _selectionOffset.X,
                        currentPoint.Y - _selectionOffset.Y
                    );
                }
                RedrawCanvas();
                return;
            }

            if (_isDrawing && _currentShape != null) {

                switch (_currentShape) {
                    case Line line:
                        line.EndPoint = currentPoint;
                        break;

                    case Rectangle rect:
                        float width = Math.Abs(currentPoint.X - _startPoint.X);
                        float height = Math.Abs(currentPoint.Y - _startPoint.Y);
                        rect.Center = new SKPoint(
                            _startPoint.X + width / 2,
                            _startPoint.Y + height / 2
                        );
                        rect.Width = width;
                        rect.Height = height;
                        break;

                    case Circle circle:
                        float radius = SKPoint.Distance(_startPoint, currentPoint);
                        circle.Center = _startPoint;
                        circle.Radius = radius;
                        break;
                }

                RedrawCanvas();
            }
        }

        private void OnPointerUp(object o, ButtonReleaseEventArgs args)
        {
            try
            {
                if (_isDragging && _selectedShape != null)
                {
                    var currentPoint = new SKPoint((float)args.Event.X, (float)args.Event.Y);
                    _cmdManager.Execute(new MoveCommand(
                        _selectedShape,
                        currentPoint.X - _selectionOffset.X - _selectedShape.Center.X,
                        currentPoint.Y - _selectionOffset.Y - _selectedShape.Center.Y
                    ));
                }

                _isDragging = false;
                _isDrawing = false;
                UpdateSelectionState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnPointerUp: {ex}");
            }
        }

        private void ShowCircleDialog(SKPoint center)
        {
            var dialog = new CircleDialog(this);

            dialog.Response += (_, e) =>
            {
                try {
                    if (e.ResponseId != ResponseType.Ok) return;

                    var circle = new Circle(center, dialog.Radius)
                    {
                        Background = new SKColor(255, 0, 0, 100),
                        BorderColor = SKColors.Black,
                        BorderWidth = 2
                    };

                    _cmdManager.Execute(new DrawCommand(_shapes, circle));
                    RedrawCanvas();
                    UpdateUndoRedoButtons();
                }
                finally
                {
                    dialog.Destroy();
                }
            };

            dialog.Show();
        }

        private void ShowRectangleDialog(SKPoint center)
        {
            var dialog = new RectangleDialog(this);

            dialog.Response += (_, e) =>
            {
                try {
                    if (e.ResponseId != ResponseType.Ok) return;

                    var rect = new Rectangle(center, dialog.Width, dialog.Height)
                    {
                        Background = new SKColor(0, 255, 0, 100),
                        BorderColor = SKColors.Black,
                        BorderWidth = 2
                    };

                    _cmdManager.Execute(new DrawCommand(_shapes, rect));
                    RedrawCanvas();
                    UpdateUndoRedoButtons();
                }
                finally
                {
                    dialog.Destroy();
                }
            };

            dialog.Show();
        }

        private void ShowTriangleDialog(SKPoint clickPoint)
        {
            var dialog = new TriangleDialog(this);

            dialog.Response += (_, e) =>
            {
                try {
                    if (e.ResponseId != ResponseType.Ok) return;

                    var triangle = new Triangle(
                        center: clickPoint,
                        sideA: dialog.FirstSide,
                        sideB: dialog.SecondSide,
                        sideC: dialog.ThirdSide)
                    {
                        Background = new SKColor(255, 165, 0, 180),
                        BorderColor = new SKColor(0, 0, 139),
                        BorderWidth = 1.5f,
                        IsSelected = false
                    };

                    _cmdManager.Execute(new DrawCommand(_shapes, triangle));
                    RedrawCanvas();
                    UpdateUndoRedoButtons();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating triangle: {ex}");
                }
                finally
                {
                    dialog.Destroy();
                }
            };

            dialog.Show();
        }

        private void RemoveShapeAt(SKPoint point)
        {
            try
            {
                var shape = _shapes.LastOrDefault(s => s.Contains(point));
                if (shape == null) return;

                _cmdManager.Execute(new EraseCommand(_shapes, shape));
                RedrawCanvas();
                UpdateUndoRedoButtons();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing shape: {ex.Message}");
            }
        }
        private void OnClearClicked(object? sender, EventArgs args)
        {
            _cmdManager.Execute(new ClearAllCommand(_shapes));
            RedrawCanvas();
            UpdateUndoRedoButtons();
        }

        private void OnSaveClicked(object? sender, EventArgs args)
        {
            using var dialog = new FileChooserDialog(
                "Save Canvas",
                this,
                FileChooserAction.Save,
                "Cancel", ResponseType.Cancel,
                "Save", ResponseType.Accept);
            dialog.DoOverwriteConfirmation = true;

            using var filter = new FileFilter();
            filter.AddPattern("*.json");
            filter.Name = "JSON Files";
            dialog.AddFilter(filter);

            string initialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            try
            {
                if (Directory.Exists(initialDir))
                {
                    dialog.SetCurrentFolder(initialDir);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error setting initial directory: {ex.Message}");
            }

            SetCurrentName(dialog, $"canvas_{DateTime.Now:yyyyMMdd_HHmmss}");

            if (dialog.Run() == (int)ResponseType.Accept)
            {
                try
                {
                    string filename = dialog.Filename;

                    if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        filename += ".json";
                    }

                    string json = CanvasSerializer.Serialize(_shapes);
                    File.WriteAllText(filename, json);
                    ShowInfo($"Canvas saved successfully to:\n{filename}");
                }
                catch (Exception ex)
                {
                    ShowErrorDialog($"Failed to save canvas:\n{ex.Message}");
                }
            }

            dialog.Destroy();
        }

        private void OnLoadClicked(object? sender, EventArgs args)
        {
            Application.Invoke((_, _) =>
            {
                var dialog = new FileChooserDialog(
                    "Open File",
                    this,
                    FileChooserAction.Open,
                    "Cancel", ResponseType.Cancel,
                    "Open", ResponseType.Accept)
                {
                    WindowPosition = WindowPosition.CenterOnParent
                };

                try
                {
                    using var filter = new FileFilter();
                    filter.AddPattern("*.json");
                    filter.Name = "JSON Files";
                    dialog.AddFilter(filter);

                    dialog.Response += (_, responseArgs) =>
                    {
                        if (responseArgs.ResponseId == ResponseType.Accept)
                        {
                            try
                            {
                                string filename = dialog.Filename;
                                if (!string.IsNullOrEmpty(filename))
                                {
                                    LoadCanvasFromFile(filename);
                                }
                            }
                            catch (Exception ex)
                            {
                                ShowErrorDialog($"Load error: {ex.Message}");
                            }
                        }

                        dialog.Destroy();
                    };

                    dialog.Show();
                }
                catch (Exception ex)
                {
                    ShowErrorDialog($"Dialog error: {ex.Message}");
                    dialog.Destroy();
                }
            });
        }

        private void LoadCanvasFromFile(string filename)
        {
            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        string json = File.ReadAllText(filename);
                        var shapes = CanvasSerializer.Deserialize(json);

                        Application.Invoke((_, _) =>
                        {
                            try
                            {
                                _cmdManager.Execute(new ReplaceAllCommand(_shapes, shapes));
                                RedrawCanvas();
                                ShowErrorDialog($"Loaded {shapes.Count} shapes");
                            }
                            catch (Exception ex)
                            {
                                ShowErrorDialog($"Failed to update canvas: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Invoke((_, _) =>
                        {
                            ShowErrorDialog($"Failed to read file: {ex.Message}");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Failed to start loading: {ex.Message}");
            }
        }

        private static void ShowInfo(string message)
            => ShowMessage("Info", message, MessageType.Info);

        private static void ShowMessage(string title, string message, MessageType messageType)
        {
            var dialog = new MessageDialog(
                null,
                DialogFlags.Modal,
                messageType,
                ButtonsType.Ok,
                message)
            {
                Title = title
            };

            dialog.Run();
            dialog.Destroy();
        }

        private void ShowErrorDialog(string message)
        {
            Application.Invoke((_, _) =>
            {
                var dialog = new MessageDialog(
                    this,
                    DialogFlags.Modal,
                    MessageType.Error,
                    ButtonsType.Ok,
                    message)
                {
                    Title = "Error",
                    WindowPosition = WindowPosition.Center
                };

                dialog.Response += (_, _) => dialog.Destroy();
                dialog.Show();
            });
        }

        protected override bool OnKeyPressEvent(EventKey evnt)
        {
            try
            {
                bool ctrlPressed = evnt.State.HasFlag(ModifierType.ControlMask);
                bool shiftPressed = evnt.State.HasFlag(ModifierType.ShiftMask);

                if (!ctrlPressed)
                    return base.OnKeyPressEvent(evnt);

                switch (evnt.Key)
                {
                    case Gdk.Key.z when !shiftPressed:
                        if (_cmdManager.CanUndo())
                        {
                            OnUndoClicked(this, EventArgs.Empty);
                            return true;
                        }
                        break;

                    case Gdk.Key.z when shiftPressed:
                    case Gdk.Key.y:
                        if (_cmdManager.CanRedo())
                        {
                            OnRedoClicked(this, EventArgs.Empty);
                            return true;
                        }
                        break;

                    case Gdk.Key.s:
                        OnSaveClicked(this, EventArgs.Empty);
                        return true;

                    case Gdk.Key.o:
                        OnLoadClicked(this, EventArgs.Empty);
                        return true;

                    case Gdk.Key.q:
                        Application.Quit();
                        return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Key press error: {ex.Message}");
                ShowErrorDialog($"Keyboard shortcut error: {ex.Message}");
            }

            return base.OnKeyPressEvent(evnt);
        }

        private static void SetCurrentName(FileChooserDialog dialog, string defaultName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(defaultName))
                {
                    dialog.CurrentName = "canvas.json";
                    return;
                }

                if (!defaultName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    defaultName += ".json";
                }

                string currentFolder = dialog.CurrentFolder;
                if (string.IsNullOrEmpty(currentFolder))
                {
                    currentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }


                string fullPath = System.IO.Path.Combine(currentFolder, defaultName);
                if (File.Exists(fullPath))
                {
                    string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(defaultName);
                    string extension = System.IO.Path.GetExtension(defaultName);
                    int counter = 1;

                    do
                    {
                        defaultName = $"{nameWithoutExt}_{counter}{extension}";
                        fullPath = System.IO.Path.Combine(currentFolder, defaultName);
                        counter++;
                    } while (File.Exists(fullPath));
                }

                dialog.CurrentName = defaultName;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in SetCurrentName: {ex.Message}");
                dialog.CurrentName = "canvas.json";
            }
        }

        private void OnFillColorClicked(object sender, EventArgs e)
        {
            if (_selectedShape == null) return;

            using var dialog = new ColorSelectionDialog("Choose fill color");

            dialog.TransientFor = this;
            dialog.Modal = true;

            var oldColor = _selectedShape.Background;
            dialog.ColorSelection.CurrentColor = new Gdk.Color(
                oldColor.Red,
                oldColor.Green,
                oldColor.Blue);

            if (dialog.Run() == (int)ResponseType.Ok)
            {
                var newColor = dialog.ColorSelection.CurrentColor;
                var skColor = new SKColor(
                    (byte)newColor.Red,
                    (byte)newColor.Green,
                    (byte)newColor.Blue);

                _cmdManager.Execute(new ChangeFillCommand(_selectedShape, skColor));
            }

            dialog.Destroy();
            RedrawCanvas();
        }
    }
}
