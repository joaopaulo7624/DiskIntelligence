namespace DiskIntelligence.Models;

public record DuplicateGroup
{
    public ulong Size { get; init; }
    public List<string> Paths { get; init; } = [];
    public ulong WastedBytes { get; init; }
}