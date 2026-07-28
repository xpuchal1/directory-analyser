namespace DirectoryAnalyzer.Models;

public class DirectoryMetadata
{
    public required DateTime LastRunTimeUtc { get; set; }
    public required IEnumerable<VersionedDirectoryItem> Items { get; set; }
}
