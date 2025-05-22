using TextEditor.Core.Documents;

namespace TextEditor.Core.Storage;

public interface IStorageStrategy {
    void Save(Document document, string path);
    Document? Load(string path);
    void Delete(string path);
}
