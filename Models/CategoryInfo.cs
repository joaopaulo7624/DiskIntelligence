namespace DiskIntelligence.Models;

public record CategoryInfo
{
    public string Name { get; init; } = "";
    public ulong Size { get; init; }
    public uint Count { get; init; }
    public string Color { get; init; } = "";
    public string Icon { get; init; } = "";
    public double Percentage { get; init; }
}