namespace TextEditor.Core.Formatting;

public class BoldTextDecorator : TextDecorator
{
    public BoldTextDecorator(ITextComponent component) : base(component) {}

    public override string ApplyFormat() => $"[BOLD]{_component.ApplyFormat()}[/BOLD]";
}
