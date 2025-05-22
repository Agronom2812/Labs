namespace TextEditor.Services;

public static class ClipboardService
{
    private static string? s_buffer = string.Empty;
    private static TextSelection? s_lastSelection;

    public static void Copy(string? text, int start, int length)
    {
        s_buffer = text;
        s_lastSelection = new TextSelection(start, length);
    }

    public static void Cut(string? text, int start, int length)
    {
        Copy(text, start, length);
        s_buffer = text.Substring(start, length);
    }

    public static string? Paste() => s_buffer;

    public static TextSelection? GetLastSelection() => s_lastSelection;

    public sealed record TextSelection(int Start, int Length);
}
