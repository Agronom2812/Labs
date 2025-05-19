using TextEditor.Core.Documents.Interfaces;

namespace TextEditor.Core.Documents;

public abstract class Document : ISavable, ILoadable, IDisplayable {
    public string Title { get; set; }
    public string Content { get; set; }

    public abstract void Save(string filePath);
    public abstract void Load(string filePath);
    public abstract void Display();
}
