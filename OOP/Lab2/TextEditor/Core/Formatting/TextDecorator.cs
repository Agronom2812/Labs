namespace TextEditor.Core.Formatting;

public abstract class TextDecorator(ITextComponent component) : ITextComponent {
    protected readonly ITextComponent Component = component;

    public virtual string GetText() => Component.GetText();
    public abstract string ApplyFormat();
}
