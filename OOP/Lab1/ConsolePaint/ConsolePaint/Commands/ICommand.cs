namespace ConsolePaint.Commands;

public interface ICommand
{
    void Execute();
    void Undo();
}
