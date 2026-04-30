namespace DiskIntelligence.Models;

public record InstalledSoftware
{
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string Publisher { get; init; } = "";
    public string InstallDate { get; init; } = "";
    public ulong EstimatedSize { get; init; }
}