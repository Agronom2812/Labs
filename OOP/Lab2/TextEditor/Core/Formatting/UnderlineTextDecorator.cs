namespace TextEditor.Core.Formatting;

public class UnderlineTextDecorator : TextDecorator {
    public UnderlineTextDecorator(ITextComponent component) : base(component) { }

    public override string ApplyFormat() => $"[UNDERLINE]{_component.ApplyFormat()}[/UNDERLINE]";
}
