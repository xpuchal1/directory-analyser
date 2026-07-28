using DirectoryAnalyzer.Helpers;
using DirectoryAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryAnalyzer.Controllers;

[ApiController]
[Route("[controller]")]
public class DirectoryController : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> AnalyzeDirectory([FromQuery] string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        if (!directoryInfo.Exists)
        {
            return BadRequest("No directory found on provided path.");
        }

        var normalizedPath = Path.TrimEndingDirectorySeparator(directoryInfo.FullName);
        var tempPath = "Temp/" + Uri.EscapeDataString(normalizedPath) + ".json";

        var metadata = await FileHelper.ReadJsonFile<DirectoryMetadata>(tempPath);
        var originalItems = metadata?.Items.ToList() ?? [];

        List<VersionedDirectoryItem> newItems = [];
        List<VersionedDirectoryItem> changedItems = [];
        List<VersionedDirectoryItem> unchangedItems = [];

        var runTimeUtc = DateTime.UtcNow;
        var items = directoryInfo.EnumerateFileSystemInfos();
        foreach (FileSystemInfo info in items)
        {
            var originalItem = originalItems.Find(i => i.Path == info.Name);

            if (originalItem is null)
            {
                newItems.Add(new VersionedDirectoryItem
                {
                    Path = info.Name,
                });
                continue;
            }

            var versionUpdated = metadata?.LastRunTimeUtc < info.LastWriteTimeUtc && info is FileInfo;

            if (versionUpdated)
            {
                changedItems.Add(new VersionedDirectoryItem
                {
                    Path = info.Name,
                    Version = originalItem.Version + 1,
                });
            }
            else
            {
                unchangedItems.Add(originalItem);
            }
        }

        var updatedItems = new DirectoryMetadata()
        {
            Items = [..newItems, ..unchangedItems, ..changedItems],
            LastRunTimeUtc = runTimeUtc,
        };

        IEnumerable<string> deletedItems = originalItems
            .Where(i => updatedItems.Items.All(u => u.Path != i.Path))
            .Select(i => i.Path);

        await FileHelper.WriteJsonFile(updatedItems, tempPath);

        return Ok(new
        {
            NewItems = newItems,
            ChangedItems = changedItems,
            DeletedItems = deletedItems,
        });
    }
}
