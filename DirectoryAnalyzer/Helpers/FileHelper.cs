using System.Text.Json;

namespace DirectoryAnalyzer.Helpers;

public static class FileHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task WriteJsonFile<T>(T obj, string relativePath) where T : class
    {
        var directory = Path.GetDirectoryName(relativePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(obj, JsonOptions);
        await File.WriteAllTextAsync(relativePath, json);
    }
    
    public static async Task<T?> ReadJsonFile<T>(string relativePath)
    {
        if (!File.Exists(relativePath))
        {
            return default;
        }
        
        await using var stream = File.OpenRead(relativePath);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
    }
}
