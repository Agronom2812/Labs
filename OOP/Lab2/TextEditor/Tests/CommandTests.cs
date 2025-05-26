using Xunit;
using TextEditor.Core.Commands;
using TextEditor.Core.Documents;
using TextEditor.Core.Services;
using TextEditor.Core.Users;

namespace TextEditor.Tests;

public sealed class CommandTests
{
    private readonly Document _doc = new PlainTextDocument(new NotificationService(new UserManager()));
    private readonly CommandHistory _history = new();

    [Fact]
    public void DeleteTextCommand_Redo_RestoresText()
    {
        // Given
        _doc.Content = "test";
        var command = new DeleteTextCommand(_doc, 0, 2);

        // When
        _history.Execute(command);
        _history.Undo();
        _history.Redo();

        // Then
        Assert.Equal("st", _doc.Content);
    }
}
