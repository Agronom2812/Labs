using Xunit;
using TextEditor.Core.Users;

namespace TextEditor.Tests;

public sealed class UserManagerTests
{
    private readonly UserManager _userManager = new();

    [Fact]
    public void AddUser_NewUser_AddsSuccessfully()
    {
        // Given
        int expectedResult = _userManager.GetAllUsers().Count() + 1;

        // When
        for (int i = 0;;) {
            while (_userManager.Exists($"test{i}")) {
                i++;
            }
            _userManager.AddUser($"test{i}", UserRole.Editor);
            break;
        }

        // Then
        Assert.Equal(expectedResult, _userManager.GetAllUsers().Count());
    }

    [Fact]
    public void Login_InvalidUser_ReturnsFalse()
    {
        // Given
        bool result = _userManager.Login("nonexistent");

        // When & Then
        Assert.False(result);
    }
}
