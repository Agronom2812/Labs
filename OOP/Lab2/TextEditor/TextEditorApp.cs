using TextEditor.Core.Commands;
using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;
using TextEditor.Core.Services;
using TextEditor.Core.Storage;
using TextEditor.Core.Users;
using TextEditor.Services;

namespace TextEditor;

public sealed class TextEditorApp
{
    private Document? _currentDocument;
    private readonly DocumentSerializerService _serializer;
    private readonly string _defaultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
    private string _currentDocumentPath = string.Empty;
    private readonly CommandHistory _history = new();
    private readonly UserManager _userManager;
    private readonly NotificationService _notificationService;

    public TextEditorApp()
    {
        _userManager = new UserManager();
        _notificationService = new NotificationService(_userManager);
        _serializer = new DocumentSerializerService(_notificationService);
    }

    public void Run()
    {
        InitializeDocumentsDirectory();
        HandleUserLogin();

        while (true)
        {
            try
            {
                HandleHotkeys();
                ShowMainMenu();
                int choice = GetUserChoice(0, GetMaxMenuOption());
                ProcessMenuChoice(choice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.ReadKey();
            }
        }
    }

    private void InitializeDocumentsDirectory()
    {
        if (!Directory.Exists(_defaultDirectory))
        {
            Directory.CreateDirectory(_defaultDirectory);
        }
    }

    private void HandleUserLogin()
    {
        while (!_userManager.IsLoggedIn)
        {
            Console.Clear();
            Console.WriteLine("=== Вход в текстовый редактор ===");
            Console.Write("Введите ваше имя: ");
            string name = Console.ReadLine()?.Trim() ?? string.Empty;

            if (_userManager.Login(name))
            {
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

    private int GetMaxMenuOption()
    {
        int baseOptions = 8;
        if (_userManager.CurrentUser?.Role == UserRole.Admin)
            return 11;
        return 10;
    }

    private void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Консольный текстовый редактор ===");
        Console.WriteLine($"Пользователь: {_userManager.CurrentUser?.Name} ({_userManager.CurrentUser?.Role})");
        Console.WriteLine($"Документ: {(_currentDocument?.Title ?? "не выбран")}");

        Console.WriteLine("1. Создать документ");
        Console.WriteLine("2. Открыть документ");

        if (_userManager.CurrentUser?.Role != UserRole.Viewer)
        {
            Console.WriteLine("3. Сохранить документ");
            Console.WriteLine("4. Удалить документ");
            Console.WriteLine("5. Переименовать документ");
        }

        Console.WriteLine("6. Показать содержимое");

        if (_userManager.CurrentUser?.Role == UserRole.Editor ||
            _userManager.CurrentUser?.Role == UserRole.Admin)
        {
            Console.WriteLine("7. Редактировать содержимое");
        }

        Console.WriteLine("8. Отменить действие (Ctrl+Z)");
        Console.WriteLine("9. Повторить действие (Ctrl+Y)");
        Console.WriteLine("10. Выйти из аккаунта");

        if (_userManager.CurrentUser?.Role == UserRole.Admin)
        {
            Console.WriteLine("11. Управление пользователями");
        }

        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    private void ProcessMenuChoice(int choice)
    {
        switch (choice)
        {
            case 1: CreateDocument(); break;
            case 2: OpenDocument(); break;
            case 3 when _userManager.CurrentUser?.Role != UserRole.Viewer: SaveDocument(); break;
            case 4 when _userManager.CurrentUser?.Role != UserRole.Viewer: DeleteCurrentDocument(); break;
            case 5 when _userManager.CurrentUser?.Role != UserRole.Viewer: RenameCurrentDocument(); break;
            case 6: DisplayDocument(); break;
            case 7 when _userManager.CurrentUser?.Role is UserRole.Editor or UserRole.Admin: HandleEditOperations();
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

    private void CreateDocument()
    {
        Console.Clear();
        Console.WriteLine("Выберите тип документа:");
        Console.WriteLine("1. Обычный текст (TXT)");
        Console.WriteLine("2. Markdown (MD)");
        Console.WriteLine("3. RichText (RTF)");
        Console.Write("Ваш выбор (1-3): ");

        int typeChoice = GetUserChoice(1, 3);
        _currentDocument = typeChoice switch
        {
            1 => new PlainTextDocument(_notificationService),
            2 => new MarkdownDocument(_notificationService),
            3 => new RichTextDocument(_notificationService),
            _ => throw new InvalidOperationException()
        };

        Console.Write("Введите имя файла: ");
        string? fileName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(fileName))
        {
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

    private void OpenDocument()
    {
        Console.Clear();
        Console.WriteLine("Выберите источник:");
        Console.WriteLine("1. Локальный файл");
        Console.WriteLine("2. Облачное хранилище");
        Console.WriteLine("3. База данных");
        Console.Write("Ваш выбор (1-3): ");

        int storageChoice = GetUserChoice(1, 3);
        var storageType = (StorageType)(storageChoice - 1);

        try
        {
            if (storageType == StorageType.Local)
            {
                Console.WriteLine("\nДоступные документы:");
                string[] files = Directory.GetFiles(_defaultDirectory);
                for (int i = 0; i < files.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");
                }

                Console.Write("\nВведите номер файла: ");
                int fileNumber = GetUserChoice(1, files.Length);
                _currentDocumentPath = files[fileNumber - 1];
            }
            else
            {
                Console.Write("\nВведите путь/ID документа: ");
                _currentDocumentPath = Console.ReadLine() ?? string.Empty;
            }

            var storage = StorageStrategyFactory.Create(storageType, _notificationService);
            _currentDocument = storage.Load(_currentDocumentPath);
            Console.WriteLine($"\nДокумент открыт: {Path.GetFileName(_currentDocumentPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОшибка: {ex.Message}");
        }
        finally
        {
            Task.Delay(1500).Wait();
        }
        _history.Clear();
    }

    private void SaveDocument()
    {
        if (_userManager.CurrentUser?.Role == UserRole.Viewer)
        {
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
        string format = formatChoice switch
        {
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

        try
        {
            if (string.IsNullOrEmpty(_currentDocumentPath))
            {
                _currentDocumentPath = Path.Combine(_defaultDirectory,
                    $"new_document.{format}");
            }

            var storage = StorageStrategyFactory.Create(storageType, _notificationService);
            storage.Save(_currentDocument, _currentDocumentPath);
            Console.WriteLine($"\nДокумент сохранен: {Path.GetFileName(_currentDocumentPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОшибка сохранения: {ex.Message}");
        }
        finally
        {
            Task.Delay(1500).Wait();
        }
    }

    // ... остальные методы (DeleteCurrentDocument, RenameCurrentDocument, DisplayDocument и т.д.)
    // должны быть реализованы аналогично с учетом проверок прав доступа

    private static int GetUserChoice(int min, int max, int maxAttempts = 3)
    {
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max)
            {
                return choice;
            }

            attempts++;
            Console.Write($"Неверный ввод. Попробуйте еще раз ({maxAttempts - attempts} попыток): ");
        }

        throw new InvalidOperationException("Превышено количество попыток ввода");
    }

    private void HandleHotkeys()
    {
        if (!Console.KeyAvailable) return;

        var key = Console.ReadKey(true);
        if ((key.Modifiers & ConsoleModifiers.Control) != 0)
        {
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
}
