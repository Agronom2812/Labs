using Application = Gtk.Application;

namespace ConsolePaint;

internal static class Program
{
    [Obsolete("Obsolete")]
    private static void Main(string[] args)
    {
        Application.Init();
        new MainWindow().ShowAll();
        Application.Run();
    }
}
