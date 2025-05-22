namespace TextEditor.Core.Users;

public class User
{
    public string Name { get; }
    public UserRole Role { get; set; }

    public User(string name, UserRole role)
    {
        Name = name;
        Role = role;
    }
}

public enum UserRole
{
    Viewer,
    Editor,
    Admin
}
