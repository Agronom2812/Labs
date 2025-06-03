using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
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
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _config;
    private readonly PermissionsRegistry _permissionsRegistry;

    public TextEditorApp() {
        _permissionsRegistry = new PermissionsRegistry();
        _userManager = new UserManager();
        _notificationService = new NotificationService(_userManager);

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("./appsettings.json", optional: true);

        _config = configBuilder.Build();

        try {
            string regionName = _config["AWS:Region"] ?? "eu-north-1";
            var region = RegionEndpoint.GetBySystemName(regionName);

            string awsAccessKey = _config["AWS:AccessKey"];
            string awsSecretKey = _config["AWS:SecretKey"];

            _s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка инициализации Amazon S3: {ex.Message}");
            _s3Client = new AmazonS3Client(RegionEndpoint.EUNorth1);
        }
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

    #region Initialization

    private void InitializeDocumentsDirectory() {
        if (!Directory.Exists(_defaultDirectory)) {
            Directory.CreateDirectory(_defaultDirectory);
        }
    }

    private void HandleUserLogin() {
        while (!_userManager.IsLoggedIn) {
            try {
                Console.Clear();
                Console.WriteLine("=== Вход в текстовый редактор ===");
                Console.Write("Введите ваше имя: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) {
                    Console.WriteLine("Имя не может быть пустым!");
                    Task.Delay(1000).Wait();
                    continue;
                }

                string name = input.Trim();

                if (_userManager.Exists(name)) {
                    _userManager.Login(name);
                    Console.WriteLine($"\nДобро пожаловать, {name}!");
                    Task.Delay(1500).Wait();
                    return;
                }

                Console.WriteLine("\nПользователь не найден. Создать нового? (y/n)");
                var key = Console.ReadKey();
                if (key.Key != ConsoleKey.Y) continue;

                _userManager.AddUser(name);
                _userManager.Login(name);
                Console.WriteLine("\nНовый пользователь создан!");
                Task.Delay(1500).Wait();
                return;
            }
            catch (Exception ex) {
                Console.WriteLine($"\nОшибка: {ex.Message}");
                Task.Delay(2000).Wait();
            }
        }
    }

    #endregion

    #region Main Menu

    private static int GetMaxMenuOption() {
        const int baseOptions = 12;
        return baseOptions;
    }

    private void ShowMainMenu() {
        Console.Clear();
        Console.WriteLine("=== Консольный текстовый редактор ===");
        Console.WriteLine($"Пользователь: {_userManager.CurrentUser?.Name}");
        Console.WriteLine($"Документ: {(_currentDocument?.Title ?? "не выбран")}");
        if (_currentDocument != null) {
            Console.WriteLine($"Владелец: {_currentDocument.Permissions?.OwnerName}");
        }

        Console.WriteLine("1. Создать документ");
        Console.WriteLine("2. Открыть документ");

        if (_currentDocument != null && CheckDocumentAccess(DocumentAccessLevel.Edit)) {
            Console.WriteLine("3. Сохранить документ");
            Console.WriteLine("4. Удалить документ");
            Console.WriteLine("5. Переименовать документ");
        }
        else {
            Console.WriteLine("3. ---");
            Console.WriteLine("4. ---");
            Console.WriteLine("5. ---");
        }

        Console.WriteLine("6. Показать содержимое");

        if (_currentDocument != null && CheckDocumentAccess(DocumentAccessLevel.Edit)) {
            Console.WriteLine("7. Редактировать содержимое");
        }
        else {
            Console.WriteLine("7. ---");
        }

        Console.WriteLine("8. Отменить действие (Ctrl+Z)");
        Console.WriteLine("9. Повторить действие (Ctrl+Y)");
        Console.WriteLine("10. Управление уведомлениями");

        if (_currentDocument != null && CheckDocumentAccess(DocumentAccessLevel.Manage)) {
            Console.WriteLine("11. Управление правами доступа");
        }
        else {
            Console.WriteLine("11. ---");
        }

        Console.WriteLine("12. Сменить пользователя");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    private void ProcessMenuChoice(int choice) {
        switch (choice) {
            case 1: CreateDocument(); break;
            case 2: OpenDocument(); break;
            case 3 when _currentDocument != null && CheckDocumentAccess(DocumentAccessLevel.Edit):
                SaveDocument(); break;
            case 4 when _currentDocument != null && CheckDocumentAccess(DocumentAccessLevel.Edit):
                DeleteCurrentDocument(); break;
            case 5 when _currentDocument != null && CheckDocumentAccess(DocumentAccessLevel.Edit):
                RenameCurrentDocument(); break;
            case 6: DisplayDocument(); break;
            case 7 when _currentDocument != null && CheckDocumentAccess(DocumentAccessLevel.Edit):
                HandleEditOperations(); break;
            case 8: UndoOperation(); break;
            case 9: RedoOperation(); break;
            case 10: ShowNotificationsMenu(); break;
            case 11 when _currentDocument != null && CheckDocumentAccess(DocumentAccessLevel.Manage):
                ManageDocumentPermissions(); break;
            case 12: SwitchUser(); break;
            case 0: Environment.Exit(0); break;
            default:
                Console.WriteLine("Неверный выбор. Попробуйте снова.");
                Task.Delay(1000).Wait();
                break;
        }
    }

    private void SwitchUser() {
        if (_currentDocument != null) {
            SavePermissions();
        }

        _userManager.Logout();
        _currentDocument = null;
        _currentDocumentPath = string.Empty;
        _history.Clear();
        HandleUserLogin();
    }

    #endregion

    #region Document Operations

    private void OpenDocument() {
        try {
            Console.Clear();
            Console.WriteLine("Выберите источник:");
            Console.WriteLine("1. Локальный файл");
            Console.WriteLine("2. S3");
            Console.WriteLine("0. Отмена");
            Console.Write("Ваш выбор (0-2): ");

            int sourceChoice = GetUserChoice(0, 2);
            if (sourceChoice == 0) return;

            string filePath;
            if (sourceChoice == 1) {
                var files = Directory.GetFiles(_defaultDirectory)
                    .Where(f => f.EndsWith(".txt") || f.EndsWith(".md") || f.EndsWith(".rtf"))
                    .ToList();

                Console.WriteLine("\nДоступные локальные файлы:");
                for (int i = 0; i < files.Count; i++) {
                    Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");
                }

                Console.WriteLine("\nИли введите полный путь к файлу");
                Console.Write("Выберите файл (0 - отмена, номер или путь): ");

                string input = Console.ReadLine()?.Trim() ?? "";
                if (input == "0") return;

                if (int.TryParse(input, out int fileNum) && fileNum >= 1 && fileNum <= files.Count) {
                    filePath = files[fileNum - 1];
                }
                else if (File.Exists(input)) {
                    filePath = input;
                }
                else {
                    Console.WriteLine("Файл не найден!");
                    Task.Delay(1500).Wait();
                    return;
                }
            }
            else {
                Console.Write("\nВведите имя файла в S3 (например: testfile.txt): ");
                string fileName = Console.ReadLine()?.Trim() ?? "";
                fileName = fileName.TrimStart('/');
                filePath = $"s3://text-ditor-bucket/{fileName}";
            }

            var storage = CreateStorageStrategy(
                sourceChoice == 1 ? StorageType.Local : StorageType.S3,
                filePath);

            _currentDocument = storage.Load(filePath);
            _currentDocumentPath = filePath;
            _history.Clear();

            LoadDocumentPermissions();

            Console.WriteLine($"\nДокумент успешно загружен!");
            Console.WriteLine($"Владелец: {_currentDocument.Permissions?.OwnerName}");
            Task.Delay(2000).Wait();
        }
        catch (Exception ex) {
            Console.WriteLine($"\nОшибка загрузки: {ex.Message}");
            Task.Delay(2000).Wait();
        }

        LoadDocumentPermissions();
        DisplayCurrentAccessInfo();
    }

    private void DisplayCurrentAccessInfo() {
        if (_currentDocument?.Permissions == null || _userManager.CurrentUser == null) return;

        Console.WriteLine($"\nВладелец: {_currentDocument.Permissions.OwnerName}");
        Console.WriteLine("Текущие права доступа:");

        foreach (var user in _userManager.GetAllUsers()) {
            if (!_currentDocument.Permissions.HasAccess(user, DocumentAccessLevel.View)) continue;

            var level = _currentDocument.Permissions.GetAccessLevel(user);
            Console.WriteLine($"- {user.Name}: {level}");
        }
    }

    private void LoadDocumentPermissions() {
        if (_currentDocument == null || _userManager.CurrentUser == null) return;

        string permFile = GetPermissionsFilePath();

        if (File.Exists(permFile)) {
            try {
                string json = File.ReadAllText(permFile);
                var data = JsonSerializer.Deserialize<PermissionFileData>(json);

                if (data != null) {
                    var owner = _userManager.GetAllUsers()
                        .FirstOrDefault(u => u.Name.Equals(data.OwnerName,
                            StringComparison.OrdinalIgnoreCase)) ?? new User(data.OwnerName);

                    _currentDocument.InitializePermissions(owner);

                    foreach (var access in data.UserAccess) {
                        var user = _userManager.GetAllUsers()
                            .FirstOrDefault(u => u.Name.Equals(access.Key, StringComparison.OrdinalIgnoreCase));

                        if (user != null) {
                            _currentDocument.Permissions?.GrantAccess(user, access.Value);
                        }
                    }

                    return;
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Ошибка загрузки прав: {ex.Message}");
            }
        }

        if (string.IsNullOrEmpty(_currentDocument.Content)) {
            _currentDocument.InitializePermissions(_userManager.CurrentUser);
        }
        else {
            Console.WriteLine("Не удалось загрузить права доступа!");
            Console.WriteLine("Документ будет доступен только для просмотра.");
            _currentDocument.InitializePermissions(new User("unknown"));
            _currentDocument.Permissions?.GrantAccess(_userManager.CurrentUser, DocumentAccessLevel.View);
            Task.Delay(2500).Wait();
        }
    }

    private string GetPermissionsFilePath() {
        return Path.ChangeExtension(_currentDocumentPath, ".perm");
    }


    private void CreateDocument() {
        if (_userManager.CurrentUser == null) return;

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

        _currentDocument.InitializePermissions(_userManager.CurrentUser);

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

    private void SaveDocument() {
        if (_currentDocument == null || _userManager.CurrentUser == null) {
            Console.WriteLine("Нет активного документа или пользователя");
            Task.Delay(1500).Wait();
            return;
        }

        if (_currentDocument is { Permissions: not null })
            _permissionsRegistry.SavePermissions(_currentDocumentPath, _currentDocument.Permissions);

        try {
            Console.WriteLine("Выберите хранилище:");
            Console.WriteLine("1. Локальное");
            Console.WriteLine("2. S3");
            Console.WriteLine("0. Отмена");
            int storageChoice = GetUserChoice(0, 2);
            if (storageChoice == 0) return;

            string extension = _currentDocument switch {
                PlainTextDocument => ".txt",
                MarkdownDocument => ".md",
                RichTextDocument => ".rtf",
                _ => ".txt"
            };

            Console.Write("\nВведите имя файла (без расширения или с нужным расширением): ");
            string fileName = Console.ReadLine()?.Trim() ?? "document";

            if (!Path.HasExtension(fileName)) {
                fileName += extension;
            }

            if (storageChoice == 2 && !fileName.StartsWith("s3://")) {
                fileName = $"s3://{_config["AWS:BucketName"]}/{fileName}";
            }

            if (!_currentDocumentPath.StartsWith("s3://")) {
                SavePermissions();
            }

            string fileExtension = Path.GetExtension(fileName).ToLower();
            if (!IsExtensionSupported(fileExtension)) {
                Console.WriteLine($"Формат {fileExtension} не поддерживается. Используйте .txt, .md или .rtf");
                Console.WriteLine("Автоматически изменяю расширение на соответствующее типу документа...");
                fileName = Path.ChangeExtension(fileName, extension);
                Console.WriteLine($"Новое имя файла: {fileName}");
                Task.Delay(2000).Wait();
            }

            var storage = CreateStorageStrategy(
                storageChoice == 1 ? StorageType.Local : StorageType.S3,
                fileName);

            storage.Save(_currentDocument, fileName);
            _currentDocumentPath = fileName;

            if (storageChoice == 1) {
                string permFile = Path.ChangeExtension(fileName, ".perm");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_currentDocument.Permissions, options);
                File.WriteAllText(permFile, json);
            }

            Console.WriteLine($"\nДокумент успешно сохранен как: {fileName}");
        }
        catch (Exception ex) {
            Console.WriteLine($"\nОШИБКА СОХРАНЕНИЯ: {ex.Message}");
        }
        finally {
            Task.Delay(2000).Wait();
        }

        SavePermissions();
    }

    private void SavePermissions() {
        if (_currentDocument?.Permissions == null) return;

        try {
            string permFile = GetPermissionsFilePath();
            var options = new JsonSerializerOptions {
                WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var data = new {
                _currentDocument.Permissions.OwnerName,
                UserAccess = _currentDocument.Permissions.GetAllAccess()
                    .ToDictionary(x => x.Key.Name, x => x.Value)
            };

            File.WriteAllText(permFile, JsonSerializer.Serialize(data, options));
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка сохранения прав: {ex.Message}");
        }
    }

    private bool IsExtensionSupported(string extension) {
        return extension switch {
            ".txt" => _currentDocument is PlainTextDocument,
            ".md" => _currentDocument is MarkdownDocument,
            ".rtf" => _currentDocument is RichTextDocument,
            _ => false
        };
    }

    private IStorageStrategy CreateStorageStrategy(StorageType type, string filePath) {
        try {
            var serializer = SerializerFactory.GetSerializer(filePath, _notificationService);

            return type switch {
                StorageType.S3 => new S3StorageStrategy(
                    new S3StorageService(_s3Client, _config),
                    serializer,
                    _notificationService),

                _ => new LocalFileStorage(_notificationService)
            };
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка инициализации хранилища: {ex.Message}");
            throw;
        }
    }

    private void DeleteCurrentDocument() {
        if (!CheckDocumentAccess(DocumentAccessLevel.Manage)) {
            Console.WriteLine("Только владелец может удалять документы");
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

            var storageType = _currentDocumentPath.StartsWith("s3://") ? StorageType.S3 : StorageType.Local;

            var storage = CreateStorageStrategy(storageType, _currentDocumentPath);
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

    private void RenameCurrentDocument() {
        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) {
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
            Console.WriteLine($"Владелец: {_currentDocument.Permissions?.Owner.Name}");
            Console.WriteLine($"Ваши права: {GetCurrentUserAccessLevel()}");
            Console.WriteLine(new string('=', 30));

            _currentDocument.Display();

            bool isSubscribed = _userManager.CurrentUser != null && _notificationService.IsSubscribed(
                _userManager.CurrentUser, _currentDocument);
            Console.WriteLine($"\nСтатус подписки: {(isSubscribed ? "✅ Подписан" : "❌ Не подписан")}");
        }

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private string GetCurrentUserAccessLevel() {
        if (_currentDocument == null || _userManager.CurrentUser == null) return "Нет доступа";

        if (_currentDocument.Permissions?.Owner.Equals(_userManager.CurrentUser) == true)
            return "Владелец (полные права)";

        if (CheckDocumentAccess(DocumentAccessLevel.Manage))
            return "Управление";

        if (CheckDocumentAccess(DocumentAccessLevel.Edit))
            return "Редактирование";

        return CheckDocumentAccess(DocumentAccessLevel.View) ? "Просмотр" : "Нет доступа";
    }

    private bool CheckDocumentAccess(DocumentAccessLevel requiredLevel) {
        if (_currentDocument == null || _userManager.CurrentUser == null)
            return false;

        if (_currentDocument.Permissions?.Owner.Equals(_userManager.CurrentUser) == true)
            return true;

        return _currentDocument.Permissions?.HasAccess(_userManager.CurrentUser, requiredLevel) ?? false;
    }

    #endregion

    #region Edit Operations

    private void HandleEditOperations() {
        if (_currentDocument == null) {
            Console.WriteLine("Документ не выбран");
            Task.Delay(1000).Wait();
            return;
        }

        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) {
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
            case 1: InsertTextOperation(); break;
            case 2: DeleteTextOperation(); break;
            case 3: CopyOperation(); break;
            case 4: PasteOperation(); break;
            case 5: CutOperation(); break;
            case 6 when _currentDocument is RichTextDocument: FormatTextOperation(); break;
            default:
                Console.WriteLine("Неверный выбор операции");
                Task.Delay(1000).Wait();
                break;
        }
    }

    private void InsertTextOperation() {
        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) return;

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
        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) return;

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
        if (!CheckDocumentAccess(DocumentAccessLevel.View)) return;

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
        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) return;

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
        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) return;

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
        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) return;

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

    #endregion

    #region Undo/Redo

    private void UndoOperation() {
        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) {
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
        if (!CheckDocumentAccess(DocumentAccessLevel.Edit)) {
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

    #endregion

    #region Notifications

    private void ShowNotificationsMenu() {
        Console.Clear();
        Console.WriteLine($"=== Управление уведомлениями: {_currentDocument?.Title} ===");
        Console.WriteLine("1. Подписаться на этот документ");
        Console.WriteLine("2. Отписаться от этого документа");
        Console.WriteLine("3. Показать мои уведомления");
        Console.WriteLine("4. Назад");
        Console.Write("Выберите действие: ");

        int choice = GetUserChoice(1, 4);
        switch (choice) {
            case 1:
                if (_userManager.CurrentUser != null)
                    _notificationService.Subscribe(_userManager.CurrentUser, _currentDocument);
                Console.WriteLine("Вы подписаны на уведомления");
                break;
            case 2:
                if (_userManager.CurrentUser != null)
                    _notificationService.Unsubscribe(_userManager.CurrentUser, _currentDocument);
                Console.WriteLine("Вы отписаны от уведомлений");
                break;
            case 3:
                ShowUserNotifications();
                break;
            case 4:
                return;
        }

        Task.Delay(1500).Wait();
    }

    private void ShowUserNotifications() {
        if (_userManager.CurrentUser != null) {
            var notifications = _notificationService
                .GetUserNotifications(_userManager.CurrentUser)
                .OrderByDescending(n => n.Timestamp)
                .ToList();

            Console.Clear();
            Console.WriteLine($"=== Ваши уведомления ({notifications.Count}) ===");

            if (notifications.Count == 0) {
                Console.WriteLine("\nУ вас нет новых уведомлений");
            }
            else {
                foreach (var notification in notifications.Take(10)) {
                    Console.WriteLine($"\n[{notification.Timestamp:dd.MM.yyyy HH:mm}]");
                    Console.WriteLine($"Документ: {notification.Document?.Title}");
                    Console.WriteLine($"Событие: {notification.Message}");
                    Console.WriteLine($"Автор: {notification.User.Name}");
                    Console.WriteLine(new string('-', 30));
                }
            }
        }

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    #endregion

    #region Permissions Management

    private void ManageDocumentPermissions() {
        if (_currentDocument == null || _userManager.CurrentUser == null) return;

        if (_currentDocument.Permissions != null
            && !_currentDocument.Permissions.Owner.Name.Equals(_userManager.CurrentUser.Name,
                StringComparison.OrdinalIgnoreCase)) {
            Console.WriteLine("Только владелец может управлять правами доступа!");
            Task.Delay(1500).Wait();
            return;
        }

        while (true) {
            Console.Clear();
            Console.WriteLine($"=== Управление правами: {_currentDocument.Title} ===");
            Console.WriteLine($"Владелец: {_currentDocument.Permissions?.Owner.Name}\n");

            var usersWithAccess = _currentDocument.Permissions?.GetAllAccess().ToList();

            if (usersWithAccess != null && usersWithAccess.Count != 0) {
                Console.WriteLine("Текущие права:");
                foreach ((User user, DocumentAccessLevel level) in usersWithAccess) {
                    Console.WriteLine($"- {user.Name}: {level}");
                }
            }
            else {
                Console.WriteLine("Нет пользователей с дополнительными правами");
            }

            Console.WriteLine("\n1. Выдать права");
            Console.WriteLine("2. Отозвать права");
            Console.WriteLine("3. Назад");
            Console.Write("Выберите действие: ");

            int choice = GetUserChoice(1, 3);
            switch (choice) {
                case 1: GrantPermissions(); break;
                case 2: RevokePermissions(); break;
                case 3: return;
            }
        }
    }

    private void GrantPermissions() {
        if (_currentDocument?.Permissions == null || _userManager.CurrentUser == null) return;

        var availableUsers = _userManager.GetAllUsers()
            .Where(u => !u.Name.Equals(_currentDocument.Permissions.Owner.Name, StringComparison.OrdinalIgnoreCase))
            .Where(u => !_currentDocument.Permissions.HasAccess(u, DocumentAccessLevel.View))
            .ToList();

        if (availableUsers.Count == 0) {
            Console.WriteLine("Нет пользователей без прав доступа!");
            Task.Delay(1500).Wait();
            return;
        }

        Console.WriteLine("\nВыберите пользователя:");
        for (int i = 0; i < availableUsers.Count; i++) {
            Console.WriteLine($"{i + 1}. {availableUsers[i].Name}");
        }

        Console.Write("Ваш выбор: ");
        int userChoice = GetUserChoice(1, availableUsers.Count);
        var selectedUser = availableUsers[userChoice - 1];

        Console.WriteLine("\nВыберите уровень доступа:");
        Console.WriteLine("1. Просмотр");
        Console.WriteLine("2. Редактирование");
        Console.WriteLine("3. Управление (кроме смены владельца)");
        int levelChoice = GetUserChoice(1, 3);
        var accessLevel = (DocumentAccessLevel)levelChoice;

        _currentDocument.Permissions.GrantAccess(selectedUser, accessLevel);
        SavePermissions();

        Console.WriteLine($"\nПользователю {selectedUser.Name} выданы права: {accessLevel}");
        _notificationService.Notify(_currentDocument,
            $"Пользователю {selectedUser.Name} выданы права: {accessLevel}");
        Task.Delay(2000).Wait();
    }

    private void RevokePermissions() {
        if (_currentDocument?.Permissions == null) return;

        var currentAccess = _currentDocument.Permissions.GetAllAccess().ToList();
        if (currentAccess.Count == 0) {
            Console.WriteLine("Нет пользователей с правами доступа");
            Task.Delay(1000).Wait();
            return;
        }

        Console.WriteLine("\nВыберите пользователя:");
        for (int i = 0; i < currentAccess.Count; i++) {
            Console.WriteLine($"{i + 1}. {currentAccess[i].Key.Name} ({currentAccess[i].Value})");
        }

        Console.Write("Ваш выбор: ");
        int userChoice = GetUserChoice(1, currentAccess.Count);
        var selectedUser = currentAccess[userChoice - 1].Key;

        _currentDocument.Permissions.RevokeAccess(selectedUser);
        Console.WriteLine($"Права пользователя {selectedUser.Name} отозваны");
        Task.Delay(1500).Wait();
    }

    #endregion

    #region Utilities

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

    #endregion
}

internal sealed class PermissionFileData(string ownerName, Dictionary<string, DocumentAccessLevel> userAccess) {
    public string OwnerName { get; init; } = ownerName;
    public Dictionary<string, DocumentAccessLevel> UserAccess { get; init; } = userAccess;
}
