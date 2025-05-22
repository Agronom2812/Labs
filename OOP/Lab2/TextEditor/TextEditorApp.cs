using TextEditor.Core.Commands;
using TextEditor.Core.Documents;
using TextEditor.Services;

namespace TextEditor;

public sealed class TextEditorApp {
    private Document? _currentDocument;
    private readonly DocumentSerializerService _serializer = new();
    private readonly string _defaultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
    private string _currentDocumentPath = string.Empty;
    private readonly CommandHistory _history = new();

    public void Run() {
        InitializeDocumentsDirectory();

        while (true) {
            try {
                HandleHotkeys();

                ShowMainMenu();
                int choice = GetUserChoice(1, 8);
                ProcessMenuChoice(choice);
            }
            catch (Exception ex) {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
    }

    private void InitializeDocumentsDirectory() {
        if (!Directory.Exists(_defaultDirectory)) {
            Directory.CreateDirectory(_defaultDirectory);
        }
    }

    private void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Консольный текстовый редактор ===");
        Console.WriteLine($"Текущий документ: {(_currentDocument?.Title ?? "не выбран")}");
        Console.WriteLine("1. Создать документ");
        Console.WriteLine("2. Открыть документ");
        Console.WriteLine("3. Сохранить документ");
        Console.WriteLine("4. Удалить документ");
        Console.WriteLine("5. Переименовать документ");
        Console.WriteLine("6. Показать содержимое");
        Console.WriteLine("7. Редактировать содержимое");
        Console.WriteLine("8. Отменить действие (Ctrl+Z)");
        Console.WriteLine("9. Повторить действие (Ctrl+Y)");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие (0-9): ");
    }

    private void CreateDocument() {
        Console.Clear();
        Console.WriteLine("Выберите тип документа:");
        Console.WriteLine("1. Обычный текст (TXT)");
        Console.WriteLine("2. Markdown (MD)");
        Console.WriteLine("3. RichText (RTF)");
        Console.Write("Ваш выбор (1-3): ");

        int typeChoice = GetUserChoice(1, 3);
        _currentDocument = typeChoice switch {
            1 => new PlainTextDocument(),
            2 => new MarkdownDocument(),
            3 => new RichTextDocument(),
            _ => throw new InvalidOperationException()
        };

        Console.Write($"Введите имя файла (относительный или абсолютный путь): ");
        string? fileName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(fileName)) {
            fileName = $"new_document{_currentDocument switch
            {
                PlainTextDocument => ".txt",
                MarkdownDocument => ".md",
                RichTextDocument => ".rtf",
                _ => ".txt"
            }}";
        }

        _currentDocumentPath = Path.IsPathRooted(fileName)
            ? fileName
            : Path.Combine(_defaultDirectory, fileName);

        _currentDocument.Title = Path.GetFileNameWithoutExtension(_currentDocumentPath);
        Console.WriteLine($"\nСоздан новый документ: {Path.GetFileName(_currentDocumentPath)}");
        Task.Delay(1500).Wait();

        _history.Clear();
    }

    private void OpenDocument() {
        Console.Clear();
        Console.WriteLine("Доступные документы:");
        string[] files = Directory.GetFiles(_defaultDirectory);
        for (int i = 0; i < files.Length; i++) {
            Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");
        }

        Console.Write("\nВведите номер файла или полный путь: ");
        string? input = Console.ReadLine();

        try {
            if (int.TryParse(input, out int number) && number > 0 && number <= files.Length) {
                _currentDocumentPath = files[number - 1];
            }
            else {
                _currentDocumentPath = Path.IsPathRooted(input)
                    ? input
                    : Path.Combine(_defaultDirectory, input ?? string.Empty);
            }

            _currentDocument = _serializer.Load(_currentDocumentPath);
            Console.WriteLine($"\nДокумент открыт: {Path.GetFileName(_currentDocumentPath)}");
        }
        catch (Exception ex) {
            Console.WriteLine($"\nОшибка: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
        _history.Clear();
    }

    private void SaveDocument() {
        Console.Clear();
        Console.WriteLine("Выберите формат сохранения:");
        Console.WriteLine("1. TXT (обычный текст)");
        Console.WriteLine("2. JSON");
        Console.WriteLine("3. XML");
        Console.Write("Ваш выбор (1-3): ");

        int formatChoice = GetUserChoice(1, 3);
        string format = formatChoice switch {
            1 => "txt",
            2 => "json",
            3 => "xml",
            _ => "txt"
        };

        try {
            if (string.IsNullOrEmpty(_currentDocumentPath)) {
                _currentDocumentPath = Path.Combine(_defaultDirectory,
                    $"new_document.{format}");
            }

            DocumentSerializerService.Save(_currentDocument, _currentDocumentPath, format);
            Console.WriteLine($"\nДокумент сохранен: {Path.GetFileName(_currentDocumentPath)}");
        }
        catch (Exception ex) {
            Console.WriteLine($"\nОшибка сохранения: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
    }

    private void DeleteCurrentDocument() {
        if (_currentDocument == null || string.IsNullOrEmpty(_currentDocumentPath)) {
            Console.WriteLine("No active document to delete");
            return;
        }

        if (Document.Exists(_currentDocumentPath)) {
            DocumentSerializerService.DeleteDocument(_currentDocumentPath);
            Console.WriteLine($"Deleted: {Path.GetFileName(_currentDocumentPath)}");
        }
        else {
            Console.WriteLine("Document file doesn't exist");
        }

        _currentDocument = null;
        _currentDocumentPath = string.Empty;
    }

    private void RenameCurrentDocument() {
        if (_currentDocument == null) {
            Console.WriteLine("No active document");
            return;
        }

        Console.Write($"New title (current: {_currentDocument.Title}): ");
        string? newTitle = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(newTitle)) return;

        _currentDocument.Title = newTitle;
        Console.WriteLine("Document renamed");
    }

    private void DisplayDocument() {
        Console.Clear();
        Console.WriteLine($"=== Содержимое документа: {Path.GetFileName(_currentDocumentPath)} ===\n");
        _currentDocument?.Display();
        Console.WriteLine("\n\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private static int GetUserChoice(int min, int max, int maxAttempts = 3) {
        int attempts = 0;
        while (attempts < maxAttempts) {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max) {
                return choice;
            }

            attempts++;
            Console.Write($"Неверный ввод. Попробуйте еще раз ({maxAttempts - attempts} попыток): ");
        }

        throw new InvalidOperationException("Превышено количество попыток ввода");
    }

    private void ProcessMenuChoice(int choice)
    {
        switch (choice)
        {
            case 1: CreateDocument(); break;
            case 2: OpenDocument(); break;
            case 3: SaveDocument(); break;
            case 4: DeleteCurrentDocument(); break;
            case 5: RenameCurrentDocument(); break;
            case 6: DisplayDocument(); break;
            case 7: HandleEditOperations(); break;
            case 8: UndoOperation(); break;
            case 9: RedoOperation(); break;
            case 0: Environment.Exit(0); break;
        }
    }

    private void InsertTextOperation() {
        Console.Write("\nВведите текст для вставки: ");
        string? text = Console.ReadLine();

        if (string.IsNullOrEmpty(text)) {
            Console.WriteLine("Текст не может быть пустым");
            return;
        }

        if (_currentDocument is { Content: not null }) {
            Console.Write($"Введите позицию (0-{_currentDocument.Content.Length}): ");
            int position = GetUserChoice(0, _currentDocument.Content.Length);

            var command = new InsertTextCommand(_currentDocument, text, position);
            _history.Execute(command);
        }

        Console.WriteLine("Текст успешно вставлен");
        Task.Delay(1000).Wait();
    }

    private void DeleteTextOperation() {
        if (_currentDocument is { Content: not null }) {
            Console.Write($"\nВведите начальную позицию (0-{_currentDocument.Content.Length - 1}): ");
            int start = GetUserChoice(0, _currentDocument.Content.Length - 1);

            Console.Write($"Введите длину (1-{_currentDocument.Content.Length - start}): ");
            int length = GetUserChoice(1, _currentDocument.Content.Length - start);

            var command = new DeleteTextCommand(_currentDocument, start, length);
            _history.Execute(command);
        }

        Console.WriteLine("Текст успешно удален");
        Task.Delay(1000).Wait();
    }

    private void HandleEditOperations() {
        Console.WriteLine("\nРедактирование:");
        Console.WriteLine("1. Вставить текст");
        Console.WriteLine("2. Удалить текст");
        Console.WriteLine("3. Копировать (Ctrl+C)");
        Console.WriteLine("4. Вставить из буфера (Ctrl+V)");
        Console.WriteLine("5. Вырезать (Ctrl+X)");
        Console.WriteLine("6. Форматировать текст");
        Console.WriteLine("7. Назад");
        Console.Write("Выберите операцию (1-7): ");

        int choice = GetUserChoice(1, 7);

        switch (choice) {
            case 1:
                InsertTextOperation();
                break;
            case 2:
                DeleteTextOperation();
                break;
            case 3:
                CopyOperation();
                break;
            case 4:
                PasteOperation();
                break;
            case 5:
                CutOperation();
                break;
            case 6:
                FormatTextOperation();
                break;
            case 7:
                return;
        }
    }

    private void CopyOperation() {
        if (_currentDocument == null) {
            Console.WriteLine("Нет активного документа");
            return;
        }

        try {
            Console.Write("Введите начальную позицию: ");
            if (_currentDocument.Content == null) return;

            int start = GetUserChoice(0, _currentDocument.Content.Length - 1);

            Console.Write($"Введите длину (макс. {_currentDocument.Content.Length - start}): ");
            int length = GetUserChoice(1, _currentDocument.Content.Length - start);

            _currentDocument.Copy(start, length);
            Console.WriteLine($"Скопировано {length} символов");
        }
        catch (ArgumentOutOfRangeException ex) {
            Console.WriteLine($"Ошибка копирования: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
    }

    private void PasteOperation() {
        if (_currentDocument == null) {
            Console.WriteLine("Нет активного документа");
            return;
        }

        try {
            Console.Write("Введите позицию для вставки: ");
            if (_currentDocument.Content != null) {
                int position = GetUserChoice(0, _currentDocument.Content.Length);

                _currentDocument.Paste(position);
            }

            Console.WriteLine("Текст вставлен из буфера");
        }
        catch (ArgumentOutOfRangeException ex) {
            Console.WriteLine($"Ошибка вставки: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
    }

    private void CutOperation() {
        if (_currentDocument == null) {
            Console.WriteLine("Нет активного документа");
            Task.Delay(1500).Wait();
            return;
        }

        try {
            Console.Write("Введите начальную позицию: ");
            if (_currentDocument.Content == null) return;

            int start = GetUserChoice(0, _currentDocument.Content.Length - 1);

            Console.Write($"Введите длину (макс. {_currentDocument.Content.Length - start}): ");
            int length = GetUserChoice(1, _currentDocument.Content.Length - start);

            _currentDocument.Cut(start, length);
            Console.WriteLine($"Вырезано {length} символов");
        }
        catch (ArgumentOutOfRangeException ex) {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
    }


    private void FormatTextOperation() {
        if (_currentDocument is not RichTextDocument richTextDoc) {
            Console.WriteLine("Форматирование доступно только для RichText документов");
            return;
        }

        if (string.IsNullOrEmpty(richTextDoc.Content)) {
            Console.WriteLine("Документ пуст. Сначала добавьте текст.");
            return;
        }

        Console.Write("Введите начальную позицию: ");
        int start = GetUserChoice(0, richTextDoc.Content.Length - 1);

        Console.Write($"Введите длину (1-{richTextDoc.Content.Length - start}): ");
        int length = GetUserChoice(1, richTextDoc.Content.Length - start);

        Console.WriteLine("Выберите формат:");
        Console.WriteLine("1. Жирный");
        Console.WriteLine("2. Курсив");
        Console.WriteLine("3. Подчеркивание");
        int formatChoice = GetUserChoice(1, 3);

        try {
            switch (formatChoice) {
                case 1:
                    richTextDoc.ApplyBold(start, length);
                    break;
                case 2:
                    richTextDoc.ApplyItalic(start, length);
                    break;
                case 3:
                    richTextDoc.ApplyUnderline(start, length);
                    break;
            }

            Console.WriteLine("Форматирование применено");
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка форматирования: {ex.Message}");
        }
    }

    private void UndoOperation() {
        Console.WriteLine(_history.Undo() ? "Последнее действие отменено" : "Нет действий для отмены");

        Task.Delay(1000).Wait();
    }

    private void RedoOperation() {
        Console.WriteLine(_history.Redo() ? "Действие восстановлено" : "Нет действий для восстановления");

        Task.Delay(1000).Wait();
    }

    private void HandleHotkeys()
    {
        if (!Console.KeyAvailable) return;

        var key = Console.ReadKey(true);

        if ((key.Modifiers & ConsoleModifiers.Control) == 0) return;

        switch (key.Key)
        {
            case ConsoleKey.Z:
                UndoOperation();
                break;
            case ConsoleKey.Y:
                RedoOperation();
                break;
        }
    }
}
