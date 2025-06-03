using Xunit;
using TextEditor.Core.Documents;
using TextEditor.Core.Storage;
using TextEditor.Core.Services;
using TextEditor.Core.Users;

namespace TextEditor.Tests;

public sealed class StorageTests
{
    [Fact]
    public void LocalFileStorage_SaveLoad_RoundtripWorks()
    {
        // Given
        var doc = new PlainTextDocument(new NotificationService(new UserManager()))
        {
            Title = "Test",
            Content = "Content"
        };
        var storage = new LocalFileStorage(new NotificationService(new UserManager()));

        // When
        storage.Save(doc, "test.txt");
        var loaded = storage.Load("test.txt");

        // Then
        Assert.Equal(doc.Content, loaded?.Content);
        File.Delete("test.txt");
    }
}
