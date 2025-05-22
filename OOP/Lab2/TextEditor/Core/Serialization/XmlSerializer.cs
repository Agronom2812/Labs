using System.Xml.Linq;
using TextEditor.Core.Documents;

namespace TextEditor.Core.Serialization;

public sealed class XmlSerializer : IDocumentSerializer {
    public string FileExtension => ".xml";

    public string Serialize(Document? document)
    {
        return new XDocument(
            new XElement("Document",
                new XElement("Title", document?.Title),
                new XElement("Content", document?.Content),
                new XElement("Type", document?.GetType().Name)
            )).ToString();
    }

    public Document Deserialize(string data)
    {
        var doc = XDocument.Parse(data);
        return doc.Root?.Element("Type")?.Value switch
        {
            nameof(PlainTextDocument) => new PlainTextDocument {
                Title = doc.Root.Element("Title")?.Value,
                Content = doc.Root.Element("Content")?.Value
            },
            _ => throw new NotSupportedException("Document type not supported")
        };
    }
}
