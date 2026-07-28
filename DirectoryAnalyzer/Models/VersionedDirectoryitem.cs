namespace DirectoryAnalyzer.Models;

public class VersionedDirectoryItem
{
    public required string Path { get; set; }
    public int Version { get; set; } = 1;
}
