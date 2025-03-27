using ConsolePaint.Shapes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsolePaint.Utilities;

public static class FileIO
    {
        private static readonly JsonSerializerOptions s_options = new()
        {
            Converters = { new ShapeJsonConverter() },
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void SaveToFile(IEnumerable<IShape> shapes, string path)
        {
        ArgumentNullException.ThrowIfNull(shapes);
        if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            try
            {
                string json = JsonSerializer.Serialize(shapes, s_options);
                File.WriteAllText(path, json);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException
                                           or PathTooLongException)
            {
                throw new IOException("Failed to save file due to filesystem error", ex);
            }
        }

        public static List<IShape> LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException("Specified file not found", path);

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<IShape>>(json, s_options)
                    ?? [];
            }
            catch (JsonException ex)
            {
                throw new JsonException("Invalid shape data format", ex);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException)
            {
                throw new IOException("Failed to read file due to filesystem error", ex);
            }
        }
    }
