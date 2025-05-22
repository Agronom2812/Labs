namespace TextEditor.Core.Formatting;

public abstract class TextDecorator : ITextComponent
{
    protected ITextComponent _component;

    protected TextDecorator(ITextComponent component) => _component = component;

    public virtual string GetText() => _component.GetText();
    public abstract string ApplyFormat();
}
