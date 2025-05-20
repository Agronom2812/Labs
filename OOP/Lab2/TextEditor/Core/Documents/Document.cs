using TextEditor.Core.Documents.Interfaces;
using TextEditor.Core.Factories;
using TextEditor.Core.Serialization;

namespace TextEditor.Core.Documents;

public abstract class Document : ISavable, ILoadable, IDisplayable {
    public string? Title { get; set; } = "Untitled";
    public string? Content { get; set; }

    public virtual void InsertText(string? text, int position) {
        throw new NotImplementedException();
    }

    public virtual void DeleteText(int start, int length) {
        throw new NotImplementedException();
    }

    public void Save(string filePath)
    {
        var serializer = SerializerFactory.GetSerializer(filePath);
        File.WriteAllText(
            EnsureExtension(filePath, serializer.FileExtension),
            serializer.Serialize(this)
        );
    }
    public Document Load(string filePath)
    {
        var serializer = SerializerFactory.GetSerializer(filePath);
        return serializer.Deserialize(File.ReadAllText(filePath));
    }
    public abstract void Display();

    private static string EnsureExtension(string path, string extension)
        => path.EndsWith(extension) ? path : path + extension;


}
