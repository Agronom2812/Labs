using TextEditor.Core.Commands;
using TextEditor.Core.Documents;
using TextEditor.Core.Factories;
using TextEditor.Core.Services;
using TextEditor.Core.Storage;
using TextEditor.Core.Users;

namespace TextEditor;

public sealed class TextEditorApp {
    private Document? _currentDocument;
    private readonly string _defaultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
    private string _currentDocumentPath = string.Empty;
    private readonly CommandHistory _history = new();
    private readonly UserManager _userManager;
    private readonly NotificationService _notificationService;

    public TextEditorApp() {
        _userManager = new UserManager();
        _notificationService = new NotificationService(_userManager);
    }

    public void Run() {
        InitializeDocumentsDirectory();
        HandleUserLogin();

        while (true) {
            try {
                HandleHotkeys();
                ShowMainMenu();
                int choice = GetUserChoice(0, GetMaxMenuOption());
                ProcessMenuChoice(choice);
            }
            catch (Exception ex) {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.ReadKey();
            }
        }
    }

    private void InitializeDocumentsDirectory() {
        if (!Directory.Exists(_defaultDirectory)) {
            Directory.CreateDirectory(_defaultDirectory);
        }
    }

    private void HandleUserLogin() {
        while (!_userManager.IsLoggedIn) {
            Console.Clear();
            Console.WriteLine("=== Вход в текстовый редактор ===");
            Console.Write("Введите ваше имя: ");
            string name = Console.ReadLine()?.Trim() ?? string.Empty;

            if (_userManager.Login(name)) {
                string greeting = _userManager.CurrentUser?.Role == UserRole.Admin
                    ? $"\nДобро пожаловать, администратор {name}!"
                    : $"\nДобро пожаловать, {name}!";

                Console.WriteLine(greeting);
                Console.WriteLine($"Ваша роль: {_userManager.CurrentUser?.Role}");
                Task.Delay(1500).Wait();
                return;
            }

            Console.WriteLine("\nПользователь не найден. Попробуйте снова.");
            Task.Delay(1000).Wait();
        }
    }

    private int GetMaxMenuOption() {
        return _userManager.CurrentUser?.Role == UserRole.Admin ? 11 : 10;
    }

    private void ShowMainMenu() {
        Console.Clear();
        Console.WriteLine("=== Консольный текстовый редактор ===");
        Console.WriteLine($"Пользователь: {_userManager.CurrentUser?.Name} ({_userManager.CurrentUser?.Role})");
        Console.WriteLine($"Документ: {(_currentDocument?.Title ?? "не выбран")}");

        Console.WriteLine("1. Создать документ");
        Console.WriteLine("2. Открыть документ");

        if (_userManager.CurrentUser?.Role != UserRole.Viewer) {
            Console.WriteLine("3. Сохранить документ");
            Console.WriteLine("4. Удалить документ");
            Console.WriteLine("5. Переименовать документ");
        }

        Console.WriteLine("6. Показать содержимое");

        if (_userManager.CurrentUser?.Role == UserRole.Editor ||
            _userManager.CurrentUser?.Role == UserRole.Admin) {
            Console.WriteLine("7. Редактировать содержимое");
        }

        Console.WriteLine("8. Отменить действие (Ctrl+Z)");
        Console.WriteLine("9. Повторить действие (Ctrl+Y)");
        Console.WriteLine("10. Выйти из аккаунта");

        if (_userManager.CurrentUser?.Role == UserRole.Admin) {
            Console.WriteLine("11. Управление пользователями");
        }

        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    private void ProcessMenuChoice(int choice) {
        switch (choice) {
            case 1: CreateDocument(); break;
            case 2: OpenDocument(); break;
            case 3 when _userManager.CurrentUser?.Role != UserRole.Viewer: SaveDocument(); break;
            case 4 when _userManager.CurrentUser?.Role != UserRole.Viewer: DeleteCurrentDocument(); break;
            case 5 when _userManager.CurrentUser?.Role != UserRole.Viewer: RenameCurrentDocument(); break;
            case 6: DisplayDocument(); break;
            case 7 when _userManager.CurrentUser?.Role is UserRole.Editor or UserRole.Admin:
                HandleEditOperations();
                break;
            case 8: UndoOperation(); break;
            case 9: RedoOperation(); break;
            case 10:
                _userManager.Logout();
                HandleUserLogin();
                break;
            case 11 when _userManager.CurrentUser?.Role == UserRole.Admin: ManageUsers(); break;
            case 0: Environment.Exit(0); break;
            default:
                Console.WriteLine("Неверный выбор. Попробуйте снова.");
                Task.Delay(1000).Wait();
                break;
        }
    }

    private void HandleEditOperations() {
        if (_currentDocument == null) {
            Console.WriteLine("Документ не выбран");
            Task.Delay(1000).Wait();
            return;
        }

        if (_userManager.CurrentUser?.Role == UserRole.Viewer) {
            Console.WriteLine("Недостаточно прав для редактирования");
            Task.Delay(1000).Wait();
            return;
        }

        while (true) {
            Console.Clear();
            Console.WriteLine($"=== Редактирование: {_currentDocument.Title} ===");
            Console.WriteLine("1. Вставить текст");
            Console.WriteLine("2. Удалить текст");
            Console.WriteLine("3. Копировать (Ctrl+C)");
            Console.WriteLine("4. Вставить из буфера (Ctrl+V)");
            Console.WriteLine("5. Вырезать (Ctrl+X)");

            if (_currentDocument is RichTextDocument) {
                Console.WriteLine("6. Форматировать текст");
                Console.WriteLine("7. Вернуться в главное меню");
                Console.Write("Выберите операцию (1-7): ");

                int choice = GetUserChoice(1, 7);
                if (choice == 7) break;
                ProcessEditOperation(choice);
            }
            else {
                Console.WriteLine("6. Вернуться в главное меню");
                Console.Write("Выберите операцию (1-6): ");

                int choice = GetUserChoice(1, 6);
                if (choice == 6) break;
                ProcessEditOperation(choice);
            }
        }
    }

    private void ProcessEditOperation(int choice) {
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
            case 6 when _currentDocument is RichTextDocument:
                FormatTextOperation();
                break;
            default:
                Console.WriteLine("Неверный выбор операции");
                Task.Delay(1000).Wait();
                break;
        }
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
            1 => new PlainTextDocument(_notificationService),
            2 => new MarkdownDocument(_notificationService),
            3 => new RichTextDocument(_notificationService),
            _ => throw new InvalidOperationException()
        };

        Console.Write("Введите имя файла: ");
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
        Console.WriteLine("Выберите источник:");
        Console.WriteLine("1. Локальный файл");
        Console.WriteLine("2. Облачное хранилище");
        Console.WriteLine("3. База данных");
        Console.Write("Ваш выбор (1-3): ");

        int storageChoice = GetUserChoice(1, 3);
        var storageType = (StorageType)(storageChoice - 1);

        try {
            if (storageType == StorageType.Local) {
                Console.WriteLine("\nДоступные документы:");
                string[] files = Directory.GetFiles(_defaultDirectory);
                for (int i = 0; i < files.Length; i++) {
                    Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");
                }

                Console.Write("\nВведите номер файла: ");
                int fileNumber = GetUserChoice(1, files.Length);
                _currentDocumentPath = files[fileNumber - 1];
            }
            else {
                Console.Write("\nВведите путь/ID документа: ");
                _currentDocumentPath = Console.ReadLine() ?? string.Empty;
            }

            var storage = StorageStrategyFactory.Create(storageType, _notificationService);
            _currentDocument = storage.Load(_currentDocumentPath);
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
        if (_userManager.CurrentUser?.Role == UserRole.Viewer) {
            Console.WriteLine("Недостаточно прав для сохранения документа");
            Task.Delay(1000).Wait();
            return;
        }

        Console.Clear();
        Console.WriteLine("Выберите формат сохранения:");
        Console.WriteLine("1. TXT (обычный текст)");
        Console.WriteLine("2. JSON");
        Console.WriteLine("3. XML");
        Console.WriteLine("4. Markdown");
        Console.Write("Ваш выбор (1-4): ");

        int formatChoice = GetUserChoice(1, 4);
        string format = formatChoice switch {
            1 => "txt",
            2 => "json",
            3 => "xml",
            4 => "md",
            _ => "txt"
        };

        Console.WriteLine("\nВыберите место сохранения:");
        Console.WriteLine("1. Локальный файл");
        Console.WriteLine("2. Облачное хранилище");
        Console.WriteLine("3. База данных");
        Console.Write("Ваш выбор (1-3): ");

        int storageChoice = GetUserChoice(1, 3);
        var storageType = (StorageType)(storageChoice - 1);

        try {
            if (string.IsNullOrEmpty(_currentDocumentPath)) {
                _currentDocumentPath = Path.Combine(_defaultDirectory,
                    $"new_document.{format}");
            }

            var storage = StorageStrategyFactory.Create(storageType, _notificationService);
            if (_currentDocument != null) storage.Save(_currentDocument, _currentDocumentPath);
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
        if (_userManager.CurrentUser?.Role != UserRole.Admin) {
            Console.WriteLine("Только администратор может удалять документы");
            Task.Delay(1000).Wait();
            return;
        }

        if (_currentDocument == null || string.IsNullOrEmpty(_currentDocumentPath)) {
            Console.WriteLine("Нет активного документа для удаления");
            Task.Delay(1000).Wait();
            return;
        }

        try {
            Console.WriteLine($"Вы уверены, что хотите удалить документ: {_currentDocumentPath}?");
            Console.WriteLine("1. Да");
            Console.WriteLine("2. Нет");
            int confirm = GetUserChoice(1, 2);

            if (confirm != 1) return;

            var storageType = GetStorageTypeForPath(_currentDocumentPath);
            var storage = StorageStrategyFactory.Create(storageType, _notificationService);
            storage.Delete(_currentDocumentPath);

            Console.WriteLine($"Документ {Path.GetFileName(_currentDocumentPath)} удален");
            _currentDocument = null;
            _currentDocumentPath = string.Empty;
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка при удалении: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
    }

    private StorageType GetStorageTypeForPath(string path) {
        if (path.StartsWith("s3://") || path.StartsWith("https://"))
            return StorageType.Cloud;
        return StorageType.Local;
    }

    private void RenameCurrentDocument() {
        if (_userManager.CurrentUser?.Role == UserRole.Viewer) {
            Console.WriteLine("Недостаточно прав для переименования");
            Task.Delay(1000).Wait();
            return;
        }

        if (_currentDocument == null) {
            Console.WriteLine("Нет активного документа");
            Task.Delay(1000).Wait();
            return;
        }

        Console.Write($"Новое название (текущее: {_currentDocument.Title}): ");
        string? newTitle = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(newTitle)) {
            _currentDocument.Title = newTitle;
            _notificationService.Notify(_currentDocument,
                $"Документ переименован на '{newTitle}'");
            Console.WriteLine("Документ переименован");
        }
        else {
            Console.WriteLine("Название не может быть пустым");
        }

        Task.Delay(1500).Wait();
    }

    private void DisplayDocument() {
        Console.Clear();
        if (_currentDocument == null) {
            Console.WriteLine("Документ не выбран");
        }
        else {
            Console.WriteLine($"=== Содержимое документа: {_currentDocument.Title} ===");
            _currentDocument.Display();

            bool isSubscribed = _userManager.CurrentUser != null && _notificationService.IsSubscribed(
                _userManager.CurrentUser, _currentDocument);
            Console.WriteLine($"\nСтатус подписки: {(isSubscribed ? "Подписан" : "Не подписан")}");
        }

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private void UndoOperation() {
        if (_userManager.CurrentUser?.Role == UserRole.Viewer) {
            Console.WriteLine("Недостаточно прав для отмены действий");
            Task.Delay(1000).Wait();
            return;
        }

        if (_history.Undo()) {
            Console.WriteLine("Последнее действие отменено");
            if (_currentDocument != null)
                _notificationService.Notify(_currentDocument,
                    "Отменено последнее действие");
        }
        else {
            Console.WriteLine("Нет действий для отмены");
        }

        Task.Delay(1000).Wait();
    }

    private void RedoOperation() {
        if (_userManager.CurrentUser?.Role == UserRole.Viewer) {
            Console.WriteLine("Недостаточно прав для повтора действий");
            Task.Delay(1000).Wait();
            return;
        }

        if (_history.Redo()) {
            Console.WriteLine("Действие восстановлено");
            if (_currentDocument != null)
                _notificationService.Notify(_currentDocument, "Восстановлено последнее действие");
        }
        else {
            Console.WriteLine("Нет действий для восстановления");
        }

        Task.Delay(1000).Wait();
    }

    private void ManageUsers() {
        Console.Clear();
        Console.WriteLine("=== Управление пользователями ===");
        Console.WriteLine("1. Добавить пользователя");
        Console.WriteLine("2. Изменить роль пользователя");
        Console.WriteLine("3. Список пользователей");
        Console.WriteLine("4. Назад");
        Console.Write("Выберите действие: ");

        int choice = GetUserChoice(1, 4);
        switch (choice) {
            case 1:
                AddNewUser();
                break;
            case 2:
                ChangeUserRole();
                break;
            case 3:
                ShowAllUsers();
                break;
            case 4:
                return;
        }
    }

    private void AddNewUser() {
        Console.Write("Введите имя нового пользователя: ");
        string name = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.WriteLine("Выберите роль:");
        Console.WriteLine("1. Viewer (только чтение)");
        Console.WriteLine("2. Editor (редактирование)");
        Console.WriteLine("3. Admin (полные права)");
        int roleChoice = GetUserChoice(1, 3);
        UserRole role = (UserRole)(roleChoice - 1);

        try {
            _userManager.AddUser(name, role);
            Console.WriteLine($"Пользователь {name} добавлен!");
            if (_currentDocument != null)
                _notificationService.Notify(_currentDocument,
                    $"Добавлен новый пользователь: {name} ({role})");
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        Task.Delay(1500).Wait();
    }

    private void ChangeUserRole() {
        var users = _userManager.GetAllUsers().ToList();
        if (!users.Any()) {
            Console.WriteLine("Нет зарегистрированных пользователей");
            Task.Delay(1000).Wait();
            return;
        }

        Console.WriteLine("Список пользователей:");
        for (int i = 0; i < users.Count; i++) {
            Console.WriteLine($"{i + 1}. {users[i].Name} ({users[i].Role})");
        }

        Console.Write("Выберите пользователя: ");
        int userIndex = GetUserChoice(1, users.Count) - 1;

        Console.WriteLine("Выберите новую роль:");
        Console.WriteLine("1. Viewer (только чтение)");
        Console.WriteLine("2. Editor (редактирование)");
        Console.WriteLine("3. Admin (полные права)");
        int roleChoice = GetUserChoice(1, 3);
        UserRole newRole = (UserRole)(roleChoice - 1);

        try {
            _userManager.ChangeUserRole(users[userIndex].Name, newRole);
            Console.WriteLine($"Роль пользователя {users[userIndex].Name} изменена на {newRole}!");
            if (_currentDocument != null)
                _notificationService.Notify(_currentDocument,
                    $"Изменена роль пользователя {users[userIndex].Name} на {newRole}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        Task.Delay(1500).Wait();
    }

    private void InsertTextOperation() {
        Console.Write("\nВведите текст для вставки: ");
        string? text = Console.ReadLine();

        if (string.IsNullOrEmpty(text)) {
            Console.WriteLine("Текст не может быть пустым");
            Task.Delay(1000).Wait();
            return;
        }

        if (_currentDocument is { Content: not null }) {
            Console.Write($"Введите позицию (0-{_currentDocument.Content.Length}): ");
            int position = GetUserChoice(0, _currentDocument.Content.Length);

            var command = new InsertTextCommand(_currentDocument, text, position);
            _history.Execute(command);

            _notificationService.Notify(_currentDocument,
                $"Вставлен текст: '{text}' на позицию {position}");
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

            _notificationService.Notify(_currentDocument,
                $"Удален текст длиной {length} с позиции {start}");
        }

        Console.WriteLine("Текст успешно удален");
        Task.Delay(1000).Wait();
    }

    private void CopyOperation() {
        try {
            Console.Write("Введите начальную позицию: ");
            if (_currentDocument is not { Content: not null }) return;

            int start = GetUserChoice(0, _currentDocument.Content.Length - 1);

            Console.Write($"Введите длину (макс. {_currentDocument.Content.Length - start}): ");
            int length = GetUserChoice(1, _currentDocument.Content.Length - start);

            _currentDocument.Copy(start, length);
            Console.WriteLine($"Скопировано {length} символов");
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка копирования: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
    }

    private void PasteOperation() {
        try {
            Console.Write("Введите позицию для вставки: ");
            if (_currentDocument is { Content: not null }) {
                int position = GetUserChoice(0, _currentDocument.Content.Length);

                _currentDocument.Paste(position);
                _notificationService.Notify(_currentDocument,
                    $"Вставлен текст из буфера на позицию {position}");
            }

            Console.WriteLine("Текст вставлен из буфера");
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка вставки: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
    }

    private void CutOperation() {
        try {
            Console.Write("Введите начальную позицию: ");
            if (_currentDocument is not { Content: not null }) return;

            int start = GetUserChoice(0, _currentDocument.Content.Length - 1);

            Console.Write($"Введите длину (макс. {_currentDocument.Content.Length - start}): ");
            int length = GetUserChoice(1, _currentDocument.Content.Length - start);

            _currentDocument.Cut(start, length);
            _notificationService.Notify(_currentDocument,
                $"Вырезано {length} символов с позиции {start}");
            Console.WriteLine($"Вырезано {length} символов");
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        finally {
            Task.Delay(1500).Wait();
        }
    }

    private void FormatTextOperation() {
        if (_currentDocument is not RichTextDocument richTextDoc) {
            Console.WriteLine("Форматирование доступно только для RichText документов");
            Task.Delay(1000).Wait();
            return;
        }

        Console.Write("Введите начальную позицию: ");
        if (richTextDoc.Content == null) return;

        int start = GetUserChoice(0, richTextDoc.Content.Length - 1);

        Console.Write($"Введите длину (1-{richTextDoc.Content.Length - start}): ");
        int length = GetUserChoice(1, richTextDoc.Content.Length - start);

        Console.WriteLine("Выберите формат:");
        Console.WriteLine("1. Жирный");
        Console.WriteLine("2. Курсив");
        Console.WriteLine("3. Подчеркивание");
        int formatChoice = GetUserChoice(1, 3);

        try {
            string formatName = formatChoice switch {
                1 => "жирный",
                2 => "курсив",
                3 => "подчеркивание",
                _ => "неизвестный"
            };

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

            _notificationService.Notify(_currentDocument,
                $"Применено {formatName} форматирование (поз.{start}, длина {length})");
            Console.WriteLine("Форматирование применено");
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка форматирования: {ex.Message}");
        }
        finally {
            Task.Delay(1000).Wait();
        }
    }

    private void ShowAllUsers() {
        var users = _userManager.GetAllUsers().ToList();
        Console.Clear();
        Console.WriteLine("=== Список пользователей ===");

        if (users.Count == 0) {
            Console.WriteLine("Пользователи не найдены");
        }
        else {
            foreach (var user in users) {
                Console.WriteLine($"- {user.Name} ({user.Role})");
            }
        }

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
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

    private void HandleHotkeys() {
        if (!Console.KeyAvailable) return;

        var key = Console.ReadKey(true);
        if ((key.Modifiers & ConsoleModifiers.Control) == 0) return;

        switch (key.Key) {
            case ConsoleKey.Z:
                UndoOperation();
                break;
            case ConsoleKey.Y:
                RedoOperation();
                break;
        }
    }
}
