using ConsolePaint.Commands;
using Moq;
using Xunit;

namespace ConsolePaint.Tests.CommandTests;

public sealed class CommandManagerTests
{
    [Fact]
    public void Undo_AfterExecute_ReversesAction()
    {
        // Given
        var manager = new CommandManager();
        var mockCmd = new Mock<ICommand>();
        manager.Execute(mockCmd.Object);

        // When
        manager.Undo();

        // Then
        mockCmd.Verify(c => c.Undo(), Times.Once);
    }

    [Fact]
    public void Redo_AfterUndo_RepeatsAction()
    {
        // Given
        var manager = new CommandManager();
        var mockCmd = new Mock<ICommand>();
        manager.Execute(mockCmd.Object);
        manager.Undo();

        // When
        manager.Redo();

        // Then
        mockCmd.Verify(c => c.Execute(), Times.Exactly(2));
    }
}
