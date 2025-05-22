namespace TextEditor.Core.Services;

public static class ClipboardService {
    private static string? s_buffer = string.Empty;

    public static void Copy(string? text) {
        s_buffer = text;
    }

    public static string? Paste() => s_buffer;
}
