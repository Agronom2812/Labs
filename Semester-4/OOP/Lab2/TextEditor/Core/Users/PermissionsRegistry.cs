using System.Text.Json;
using TextEditor.Core.Documents;

namespace TextEditor.Core.Users;

public sealed class PermissionsRegistry {
    private const string RegistryFile = "permissions_registry.json";
    private Dictionary<string, DocumentPermissions> _registry = new();

    public PermissionsRegistry() {
        LoadRegistry();
    }

    public void SavePermissions(string documentPath, DocumentPermissions permissions) {
        string key = Path.GetFileName(documentPath);
        _registry[key] = permissions;
        SaveRegistry();

        string permFile = Path.ChangeExtension(documentPath, ".perm");
        File.WriteAllText(permFile, JsonSerializer.Serialize(permissions));
    }

    private void LoadRegistry() {
        if (!File.Exists(RegistryFile)) return;

        try {
            string json = File.ReadAllText(RegistryFile);
            _registry = JsonSerializer.Deserialize<Dictionary<string, DocumentPermissions>>(json)
                        ?? new Dictionary<string, DocumentPermissions>();
        }
        catch {
            _registry = new Dictionary<string, DocumentPermissions>();
        }
    }

    private void SaveRegistry() {
        string json = JsonSerializer.Serialize(_registry);
        File.WriteAllText(RegistryFile, json);
    }
}
