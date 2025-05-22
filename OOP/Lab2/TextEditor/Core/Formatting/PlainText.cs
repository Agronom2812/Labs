namespace TextEditor.Core.Formatting;

public class PlainText(string text) : ITextComponent {
    public string GetText() => text;
    public string ApplyFormat() => text;
}
