namespace DiskIntelligence.Models;

public record ScanProgress
{
    public string CurrentPath { get; init; } = "";
    public uint FilesScanned { get; init; }
    public uint DirsScanned { get; init; }
    public ulong TotalSize { get; init; }
}