namespace TextEditor.Core.Formatting;

public interface ITextComponent
{
    string GetText();
    string ApplyFormat();
}
