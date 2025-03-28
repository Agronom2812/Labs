using System.Text.Json;
using Gtk;
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
        private readonly List<IShape> _shapes = new();
        private SKPoint _startPoint;
        private DrawingTool _currentTool = DrawingTool.Selector;
        private SKSurface? _surface;
        private IShape? _currentShape;
        private bool _isDrawing;
        private IShape? _selectedShape;
        private SKPoint _selectionOffset;
        private bool _isDragging;

        public MainWindow() : base("Console Paint")
        {
            SetDefaultSize(800, 600);
            DeleteEvent += (_, __) => Application.Quit();

            var toolbar = new Box(Orientation.Horizontal, 5)
            {
                Margin = 5,
                Halign = Align.Start
            };

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

        private void AddToolButton(Box container, DrawingTool tool, string label)
        {
            var button = new Button
            {
                Label = label,
                Margin = 2,
                Expand = false
            };

            button.Clicked += (_, __) =>
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

        private void OnCanvasDrawn(object sender, DrawnArgs args)
        {
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

            if (_currentTool == DrawingTool.Selector)
            {
                _selectedShape = _shapes.LastOrDefault(s => s.Contains(point));

                if (_selectedShape != null)
                {
                    _selectionOffset = new SKPoint(
                        point.X - _selectedShape.Center.X,
                        point.Y - _selectedShape.Center.Y
                    );
                    _isDragging = true;
                }
                return;
            }

            _startPoint = point;
            _isDrawing = true;

            switch (_currentTool)
            {
                case DrawingTool.Line:
                    _currentShape = new Line(point, point);
                    _shapes.Add(_currentShape);
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
                    _currentShape = _shapes.LastOrDefault(s => s.Contains(point));
                    break;
            }

            if (_currentTool == DrawingTool.Selector)
            {
                foreach (var shape in _shapes)
                {
                    shape.IsSelected = false;
                }

                _selectedShape = _shapes.LastOrDefault(s => s.Contains(point));

                if (_selectedShape != null)
                {
                    _selectedShape.IsSelected = true;

                    if (_selectedShape is Line line)
                    {
                        SKPoint closestPoint = GetClosestPointOnLine(line.StartPoint, line.EndPoint, point);
                        _selectionOffset = new SKPoint(
                            point.X - closestPoint.X,
                            point.Y - closestPoint.Y
                        );
                    }
                    else
                    {
                        _selectionOffset = new SKPoint(
                            point.X - _selectedShape.Center.X,
                            point.Y - _selectedShape.Center.Y
                        );
                    }

                    _isDragging = true;
                }
                RedrawCanvas();
                return;
            }

            RedrawCanvas();
        }

        private SKPoint GetClosestPointOnLine(SKPoint start, SKPoint end, SKPoint point)
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
            if (_currentTool == DrawingTool.Selector && _isDragging && _selectedShape != null)
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
            _selectedShape = null;
            _currentShape = null;
        }

        private void ShowCircleDialog(SKPoint center)
        {
            var dialog = new CircleDialog(this);

            dialog.Response += (sender, e) =>
            {
                try
                {
                    if (e.ResponseId == ResponseType.Ok)
                    {
                        var circle = new Circle(center, dialog.Radius)
                        {
                            Background = new SKColor(255, 0, 0, 100),
                            BorderColor = SKColors.Black,
                            BorderWidth = 2
                        };

                        _cmdManager.Execute(new DrawCommand(_shapes, circle));
                        RedrawCanvas();
                    }
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

            dialog.Response += (sender, e) =>
            {
                try
                {
                    if (e.ResponseId == ResponseType.Ok)
                    {
                        var rect = new Rectangle(center, dialog.Width, dialog.Height)
                        {
                            Background = new SKColor(0, 255, 0, 100),
                            BorderColor = SKColors.Black,
                            BorderWidth = 2
                        };

                        _cmdManager.Execute(new DrawCommand(_shapes, rect));
                        RedrawCanvas();
                    }
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

            dialog.Response += (sender, e) =>
            {
                try {
                    if (e.ResponseId != ResponseType.Ok) return;

                    var triangle = new Triangle(
                        center: clickPoint,
                        firstSide: dialog.FirstSide,
                        secondSide: dialog.SecondSide,
                        thirdSide: dialog.ThirdSide,
                        vertices: null
                    )
                    {
                        Background = new SKColor(255, 0, 0, 100),
                        BorderColor = SKColors.Black,
                        BorderWidth = 2
                    };

                    _cmdManager.Execute(new DrawCommand(_shapes, triangle));
                    RedrawCanvas();
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
                if (shape != null)
                {
                    _cmdManager.Execute(new EraseCommand(_shapes, shape));
                    RedrawCanvas();
                }
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

            // Настройка фильтров файлов
            using var filter = new FileFilter();
            filter.AddPattern("*.json");
            filter.Name = "JSON Files";
            dialog.AddFilter(filter);

            // Установка начальной директории
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

            // Установка имени файла по умолчанию
            SetCurrentName(dialog, $"canvas_{DateTime.Now:yyyyMMdd_HHmmss}");

            if (dialog.Run() == (int)ResponseType.Accept)
            {
                try
                {
                    string filename = dialog.Filename;

                    // Двойная проверка расширения
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
            Application.Invoke((s, e) =>
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
                    // Настройка фильтра
                    using var filter = new FileFilter();
                    filter.AddPattern("*.json");
                    filter.Name = "JSON Files";
                    dialog.AddFilter(filter);

                    dialog.Response += (o, responseArgs) =>
                    {
                        if (responseArgs.ResponseId == ResponseType.Accept)
                        {
                            try
                            {
                                // Получаем имя файла из самого диалога
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

                        // Важно: уничтожаем диалог после использования
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

        private void LoadFile()
        {
            var dialog = new FileChooserDialog(
                "Open File",
                null,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);

            try
            {
                // Простейший фильтр файлов
                var filter = new FileFilter();
                filter.AddPattern("*.json");
                dialog.AddFilter(filter);

                if (dialog.Run() == (int)ResponseType.Accept)
                {
                    try
                    {
                        string json = File.ReadAllText(dialog.Filename);
                        var shapes = JsonSerializer.Deserialize<List<IShape>>(json);

                        _shapes.Clear();
                        _shapes.AddRange(shapes);
                        RedrawCanvas();
                        ShowInfo($"Loaded {shapes.Count} shapes");
                    }
                    catch (Exception ex)
                    {
                        ShowErrorDialog($"Load error: {ex.Message}");
                    }
                }
            }
            finally
            {
                dialog.Destroy();
            }
        }

        private void LoadCanvasFromFile(string filename)
        {
            try
            {
                // Чтение файла в фоновом потоке
                Task.Run(() =>
                {
                    try
                    {
                        string json = File.ReadAllText(filename);
                        var shapes = CanvasSerializer.Deserialize(json);

                        Application.Invoke((s, e) =>
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
                        Application.Invoke((s, e) =>
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

        private void ShowInfo(string message)
            => ShowMessage("Info", message, MessageType.Info);

        private void ShowMessage(string title, string message, MessageType messageType)
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
            Application.Invoke((s, e) =>
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

                dialog.Response += (o, args) => dialog.Destroy();
                dialog.Show();
            });
        }

        protected override bool OnKeyPressEvent(EventKey evnt)
        {
            if (evnt.State.HasFlag(ModifierType.ControlMask))
            {
                switch (evnt.Key)
                {
                    case Gdk.Key.s:
                        OnSaveClicked(null, EventArgs.Empty);
                        return true;
                    case Gdk.Key.o:
                        OnLoadClicked(null, EventArgs.Empty);
                        return true;
                }
            }
            return base.OnKeyPressEvent(evnt);
        }
        private void SetCurrentName(FileChooserDialog dialog, string defaultName)
        {
            try
            {
                // Убедимся, что имя файла не пустое
                if (string.IsNullOrWhiteSpace(defaultName))
                {
                    dialog.CurrentName = "canvas.json";
                    return;
                }

                // Добавим расширение .json, если его нет
                if (!defaultName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    defaultName += ".json";
                }

                // Получим текущую директорию (без использования Uri)
                string currentFolder = dialog.CurrentFolder;
                if (string.IsNullOrEmpty(currentFolder))
                {
                    currentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                // Генерация уникального имени файла
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
    }
}
