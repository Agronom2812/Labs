namespace TextEditor.Core.Users;

public sealed class User(string name, UserRole role) {
    public string Name { get; } = name;
    public UserRole Role { get; set; } = role;
}

public enum UserRole {
    Viewer,
    Editor,
    Admin
}
