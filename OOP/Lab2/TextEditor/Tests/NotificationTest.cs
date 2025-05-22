using Xunit;
using TextEditor.Core.Documents;
using TextEditor.Core.Services;
using TextEditor.Core.Users;

namespace TextEditor.Tests;

public sealed class NotificationTests
{
    [Fact]
    public void NotificationService_Subscribe_SavesSubscription()
    {
        // Given
        var userManager = new UserManager();
        var service = new NotificationService(userManager);
        var doc = new PlainTextDocument(service);
        userManager.Login("admin");

        // When
        service.Subscribe(userManager.CurrentUser!, doc);

        // Then
        Assert.True(service.IsSubscribed(userManager.CurrentUser!, doc));
    }
}
