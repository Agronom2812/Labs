using System.Text.Json;

namespace TextEditor.Core.Users;

public sealed class UserManager {
    private const string UsersFile = "users.json";
    private List<User> _users = [];

    public UserManager() {
        LoadUsers();

        if (_users.Any(u => u.Role == UserRole.Admin)) return;

        _users.Add(new User("admin", UserRole.Admin));
        SaveUsers();
    }

    public User? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser != null;

    public bool Login(string name) {
        CurrentUser = _users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return CurrentUser != null;
    }

    public void Logout() => CurrentUser = null;

    public void AddUser(string name, UserRole role) {
        if (_users.Any(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("User already exists");

        _users.Add(new User(name, role));
        SaveUsers();
    }

    public void ChangeUserRole(string name, UserRole newRole) {
        var user = _users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                   ?? throw new InvalidOperationException("User not found");

        user.Role = newRole;
        SaveUsers();
    }

    public IEnumerable<User> GetAllUsers() => _users.AsReadOnly();

    private void LoadUsers() {
        if (!File.Exists(UsersFile)) return;

        try {
            string json = File.ReadAllText(UsersFile);
            _users = JsonSerializer.Deserialize<List<User>>(json) ?? [];
        }
        catch {
            _users = [];
        }
    }

    private void SaveUsers() {
        string json = JsonSerializer.Serialize(_users);
        File.WriteAllText(UsersFile, json);
    }
}
