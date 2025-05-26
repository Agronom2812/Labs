namespace TextEditor.Core.Users;

public sealed class User(string name) {
    public string Name { get; } = name.Trim() ?? throw new ArgumentNullException(nameof(name));

    public override bool Equals(object? obj) {
        return obj is User user &&
               string.Equals(Name, user.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }
}
