namespace DiskIntelligence.Models;

public record ScanResult
{
    public ulong TotalSize { get; init; }
    public uint TotalFiles { get; init; }
    public uint TotalDirs { get; init; }
    public long DurationMs { get; init; }
    public string ScanPath { get; init; } = "";

    public uint TinyFiles { get; init; }
    public uint SmallFiles { get; init; }
    public uint MediumFiles { get; init; }
    public uint LargeFiles { get; init; }
    public uint HugeFiles { get; init; }

    public List<CategoryInfo> Categories { get; init; } = [];
    public List<BigFile> TopFiles { get; init; } = [];
    public List<ExtensionStat> TopExtensions { get; init; } = [];
    public List<string> EmptyDirs { get; init; } = [];
    public uint EmptyDirsCount { get; init; }
    public List<DuplicateGroup> PotentialDuplicates { get; init; } = [];
    public ulong PotentialWasted { get; init; }
    public string DeepestPath { get; init; } = "";
    public int MaxDepth { get; init; }
    public string OldestFile { get; init; } = "";
    public string NewestFile { get; init; } = "";
}