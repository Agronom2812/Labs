namespace TextEditor.Core.Formatting;

public class ItalicTextDecorator : TextDecorator
{
    public ItalicTextDecorator(ITextComponent component) : base(component) {}

    public override string ApplyFormat() => $"[ITALIC]{_component.ApplyFormat()}[/ITALIC]";
}
