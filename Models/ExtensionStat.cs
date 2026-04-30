namespace DiskIntelligence.Models;

public record ExtensionStat
{
    public string Extension { get; init; } = "";
    public uint Count { get; init; }
    public ulong Size { get; init; }
}