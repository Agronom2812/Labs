using System.Xml.Linq;
using TextEditor.Core.Documents;
using TextEditor.Core.Notifications;

namespace TextEditor.Core.Serialization;

public sealed class XmlSerializer : IDocumentSerializer
{
    private readonly INotificationService _notificationService;

    public XmlSerializer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

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

    public Document? Deserialize(string data)
    {
        var doc = XDocument.Parse(data);
        return doc.Root?.Element("Type")?.Value switch
        {
            nameof(PlainTextDocument) => new PlainTextDocument(_notificationService) {
                Title = doc.Root?.Element("Title")?.Value,
                Content = doc.Root?.Element("Content")?.Value
            },
            nameof(MarkdownDocument) => new MarkdownDocument(_notificationService) {
                Title = doc.Root?.Element("Title")?.Value,
                Content = doc.Root?.Element("Content")?.Value
            },
            _ => throw new NotSupportedException("Document type not supported")
        };
    }
}
