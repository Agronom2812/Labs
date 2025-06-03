using System.Text.Json.Serialization;
using TextEditor.Core.Users;

namespace TextEditor.Core.Documents;

[Serializable]
[method: JsonConstructor]
public sealed class DocumentPermissions(string ownerName, Dictionary<string, DocumentAccessLevel> userAccess) {
    [JsonPropertyName("ownerName")] public string OwnerName { get; } = ownerName;

    [JsonPropertyName("userAccess")] private readonly Dictionary<string, DocumentAccessLevel> _userAccess = userAccess;

    public DocumentPermissions(User owner) : this(owner.Name, new Dictionary<string, DocumentAccessLevel>())
    { }

    [JsonIgnore] public User Owner => new(OwnerName);

    public void GrantAccess(User user, DocumentAccessLevel level) {
        if (user.Name.Equals(OwnerName, StringComparison.OrdinalIgnoreCase)) return;
        _userAccess[user.Name] = level;
    }

    public void RevokeAccess(User user) {
        _userAccess.Remove(user.Name);
    }

    public bool HasAccess(User user, DocumentAccessLevel requiredLevel) {
        if (user.Name.Equals(OwnerName, StringComparison.OrdinalIgnoreCase))
            return true;

        return _userAccess.TryGetValue(user.Name, out var level) && level >= requiredLevel;
    }

    public DocumentAccessLevel GetAccessLevel(User user) {
        return user.Name.Equals(OwnerName, StringComparison.OrdinalIgnoreCase)
            ? DocumentAccessLevel.Manage
            : _userAccess.GetValueOrDefault(user.Name, DocumentAccessLevel.None);
    }

    public IEnumerable<KeyValuePair<User, DocumentAccessLevel>> GetAllAccess() {
        return _userAccess.Select(entry => new KeyValuePair<User,
            DocumentAccessLevel>(new User(entry.Key), entry.Value));
    }
}

public enum DocumentAccessLevel {
    None,
    View,
    Edit,
    Manage
}
