using TextEditor.Core.Documents;
using TextEditor.Core.Factories;

namespace TextEditor;

public sealed class TextEditorApp
{
    private Document _currentDocument;
    private readonly DocumentFactory _documentFactory = new DocumentFactory();

    public void Run()
    {
        Console.WriteLine("Welcome to Console Text Editor!");
        Console.WriteLine("Type 'help' for available commands.");

        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            ProcessCommand(input);
        }
    }

    private void ProcessCommand(string command)
    {
        string[] parts = command.Split(' ');
        string cmd = parts[0].ToLower();

        try
        {
            switch (cmd)
            {
                case "help":
                    ShowHelp();
                    break;
                case "new":
                    CreateNewDocument(parts);
                    break;
                case "open":
                    OpenDocument(parts);
                    break;
                case "save":
                    SaveDocument(parts);
                    break;
                case "exit":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine($"Unknown command: {cmd}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("help - Show this help");
        Console.WriteLine("new <type> - Create new document (types: plaintext, markdown, richtext)");
        Console.WriteLine("open <path> - Open document from file");
        Console.WriteLine("save <path> - Save document to file");
        Console.WriteLine("exit - Exit the program");
    }

    private void CreateNewDocument(string[] parts)
    {
        if (parts.Length < 2)
            throw new ArgumentException("Document type not specified");

        _currentDocument = _documentFactory.CreateDocument(parts[1]);
        _currentDocument.Title = "Untitled";
        _currentDocument.Content = string.Empty;
        Console.WriteLine($"Created new {parts[1]} document");
    }

    private void OpenDocument(string[] parts)
    {
        if (parts.Length < 2)
            throw new ArgumentException("File path not specified");

        string extension = Path.GetExtension(parts[1]).ToLower();
        _currentDocument = extension switch {
            ".txt" => _documentFactory.CreateDocument("plaintext"),
            ".md" => _documentFactory.CreateDocument("markdown"),
            _ => throw new ArgumentException("Unsupported file format")
        };

        _currentDocument.Load(parts[1]);
        Console.WriteLine($"Opened document from {parts[1]}");
    }

    private void SaveDocument(string[] parts)
    {
        if (_currentDocument == null)
            throw new InvalidOperationException("No document is currently open");

        if (parts.Length < 2)
            throw new ArgumentException("File path not specified");

        _currentDocument.Save(parts[1]);
        Console.WriteLine($"Document saved to {parts[1]}");
    }
}
