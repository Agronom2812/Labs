namespace TextEditor.Core.Documents.Interfaces;

public interface ILoadable {
    Document Load(string filePath);
}
