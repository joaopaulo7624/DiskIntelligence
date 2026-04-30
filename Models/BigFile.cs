namespace DiskIntelligence.Models;

public record BigFile : IComparable<BigFile>
{
    public string Path { get; init; } = "";
    public string Name { get; init; } = "";
    public ulong Size { get; init; }
    public string Extension { get; init; } = "";
    public string Category { get; init; } = "";

    public int CompareTo(BigFile? other) => other?.Size.CompareTo(Size) ?? 1;
}