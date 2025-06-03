using System.Text.Json;

namespace TextEditor.Core.Users;

public sealed class UserManager {
    private const string UsersFile = "users.json";
    private readonly List<User> _users;

    public UserManager() {
        _users = LoadUsersSafe();
        EnsureAdminExists();
    }

    public User? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser != null;

    public void Login(string name) {
        if (string.IsNullOrWhiteSpace(name)) return;

        CurrentUser = _users.Find(u =>
            string.Equals(u.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void Logout() => CurrentUser = null;

    public bool Exists(string name) {
        return !string.IsNullOrWhiteSpace(name) &&
               _users.Exists(u =>
                   string.Equals(u.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void AddUser(string name) {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя пользователя не может быть пустым");

        name = name.Trim();
        if (Exists(name))
            throw new InvalidOperationException("Пользователь уже существует");

        _users.Add(new User(name));
        SaveUsersSafe();
    }

    public IEnumerable<User> GetAllUsers() => _users.AsReadOnly();

    private static List<User> LoadUsersSafe() {
        try {
            if (!File.Exists(UsersFile))
                return [];

            string json = File.ReadAllText(UsersFile);
            return JsonSerializer.Deserialize<List<User>>(json) ?? [];
        }
        catch {
            return [];
        }
    }

    private void SaveUsersSafe() {
        try {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_users, options);
            File.WriteAllText(UsersFile, json);
        }
        catch (Exception ex) {
            Console.WriteLine($"Ошибка сохранения пользователей: {ex.Message}");
        }
    }

    private void EnsureAdminExists() {
        if (_users.Any(u => u.Name.Equals("admin", StringComparison.OrdinalIgnoreCase))) return;

        _users.Add(new User("admin"));
        SaveUsersSafe();
    }
}
