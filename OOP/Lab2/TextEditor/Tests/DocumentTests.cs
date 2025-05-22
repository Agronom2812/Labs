using Xunit;
using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;
using TextEditor.Core.Services;
using TextEditor.Core.Users;

namespace TextEditor.Tests;

public sealed class DocumentTests
{
    private readonly INotificationService _notificationService = new NotificationService(new UserManager());

    [Fact]
    public void PlainTextDocument_InsertDelete_WorksCorrectly()
    {
        // Given
        var doc = new PlainTextDocument(_notificationService);

        // When
        doc.InsertText("Hello", 0);
        doc.DeleteText(0, 2);

        // Then
        Assert.Equal("llo", doc.Content);
    }

    [Fact]
    public void MarkdownDocument_Display_FormatsHeaders()
    {
        // Given
        var doc = new MarkdownDocument(_notificationService)
        {
            Content = "# Header"
        };

        // When & Then
        Assert.NotNull(doc.Display);
    }

    [Fact]
    public void RichTextDocument_ApplyFormat_ChangesTextStyle()
    {
        // Given
        var doc = new RichTextDocument(_notificationService);
        doc.InsertText("Test", 0);

        // When
        doc.ApplyBold(0, 4);

        // Then
        Assert.Single(doc.Formats);
    }
}
