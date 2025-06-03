using Xunit;
using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;
using TextEditor.Core.Serialization;
using TextEditor.Core.Services;
using TextEditor.Core.Users;

namespace TextEditor.Tests;

public sealed class SerializerTests
{
    private readonly INotificationService _notificationService = new NotificationService(new UserManager());

    [Fact]
    public void JsonSerializer_Serialize_ProducesValidJson()
    {
        // Given
        var doc = new PlainTextDocument(new NotificationService(new UserManager()))
        {
            Title = "Test",
            Content = "Content"
        };
        var serializer = new JsonDocumentSerializer(_notificationService);

        // When
        string json = serializer.Serialize(doc);

        // Then
        Assert.Contains("\"Title\":\"Test\"", json);
    }
}
