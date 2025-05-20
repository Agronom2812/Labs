using TextEditor.Core.Documents;
using TextEditor.Core.Factories;
using TextEditor.Services;

namespace TextEditor;

public sealed class TextEditorApp
{
    private Document? _currentDocument;
    private readonly DocumentSerializerService _serializer = new();

    public void Run()
    {
        Console.WriteLine("=== Консольный текстовый редактор ===");

        while (true)
        {
            ShowMainMenu();
            int choice = GetUserChoice(1, 6);

            try
            {
                ProcessMenuChoice(choice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.ReadKey();
            }
        }
    }

    private static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("Главное меню:");
        Console.WriteLine("1. Создать новый документ");
        Console.WriteLine("2. Открыть документ");
        Console.WriteLine("3. Сохранить документ");
        Console.WriteLine("4. Показать документ");
        Console.WriteLine("5. Редактировать документ");
        Console.WriteLine("6. Выход");
        Console.Write("Выберите действие (1-6): ");
    }

    private static int GetUserChoice(int min, int max)
    {
        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || choice < min || choice > max)
        {
            Console.Write($"Введите число от {min} до {max}: ");
        }
        return choice;
    }

    private void ProcessMenuChoice(int choice)
    {
        switch (choice)
        {
            case 1: CreateDocument(); break;
            case 2: OpenDocument(); break;
            case 3: SaveDocument(); break;
            case 4: DisplayDocument(); break;
            case 5: EditDocument(); break;
            case 6: Environment.Exit(0); break;
        }
    }

    private void CreateDocument()
    {
        Console.Clear();
        Console.WriteLine("Выберите тип документа:");
        Console.WriteLine("1. Обычный текст");
        Console.WriteLine("2. Markdown");
        Console.WriteLine("3. RichText");
        Console.Write("Ваш выбор (1-3): ");

        int typeChoice = GetUserChoice(1, 3);
        _currentDocument = typeChoice switch
        {
            1 => new PlainTextDocument(),
            2 => new MarkdownDocument(),
            3 => new RichTextDocument(),
            _ => throw new InvalidOperationException()
        };

        Console.WriteLine($"Создан новый документ типа: {_currentDocument.GetType().Name}");
        Console.ReadKey();
    }

    private void OpenDocument()
    {
        Console.Clear();
        Console.Write("Введите путь к файлу: ");
        string? path = Console.ReadLine();

        _currentDocument = _serializer.Load(path);
        Console.WriteLine($"Документ загружен: {Path.GetFileName(path)}");
        Console.ReadKey();
    }

    private void SaveDocument()
    {
        if (_currentDocument == null)
            throw new InvalidOperationException("Нет активного документа");

        Console.Clear();
        Console.Write("Введите путь для сохранения: ");
        string? path = Console.ReadLine();

        Console.WriteLine("Выберите формат:");
        Console.WriteLine("1. TXT");
        Console.WriteLine("2. JSON");
        Console.WriteLine("3. XML");
        Console.Write("Ваш выбор (1-3): ");

        int formatChoice = GetUserChoice(1, 3);
        string format = formatChoice switch
        {
            1 => "txt",
            2 => "json",
            3 => "xml",
            _ => throw new InvalidOperationException()
        };

        _serializer.Save(_currentDocument, path, format);
        Console.WriteLine($"Документ сохранен как {format}");
        Console.ReadKey();
    }

    private void DisplayDocument()
    {
        if (_currentDocument == null)
            throw new InvalidOperationException("Нет активного документа");

        Console.Clear();
        _currentDocument.Display();
        Console.WriteLine("\nНажмите любую клавишу...");
        Console.ReadKey();
    }

    private void EditDocument()
    {
        if (_currentDocument == null)
            throw new InvalidOperationException("Нет активного документа");

        Console.Clear();
        Console.WriteLine("Текущее содержимое:");
        _currentDocument.Display();

        Console.WriteLine("\nКоманды редактирования:");
        Console.WriteLine("1. Вставить текст");
        Console.WriteLine("2. Удалить текст");
        Console.Write("Ваш выбор (1-2): ");

        int editChoice = GetUserChoice(1, 2);

        if (editChoice == 1)
        {
            Console.Write("Введите текст для вставки: ");
            string? text = Console.ReadLine();
            Console.Write("Введите позицию: ");
            int pos = GetUserChoice(0, int.MaxValue);
            _currentDocument.InsertText(text, pos);
        }
        else
        {
            Console.Write("Введите начальную позицию: ");
            int start = GetUserChoice(0, int.MaxValue);
            Console.Write("Введите длину: ");
            int length = GetUserChoice(1, int.MaxValue);
            _currentDocument.DeleteText(start, length);
        }

        Console.WriteLine("Изменения применены!");
        Console.ReadKey();
    }
}
