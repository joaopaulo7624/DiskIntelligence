namespace DiskIntelligence.Helpers;

public static class ByteFormatter
{
    private static readonly string[] _units = ["B", "KB", "MB", "GB", "TB"];

    public static string Format(ulong bytes, int decimals = 1)
    {
        if (bytes == 0) return "0 B";

        var order = Math.Min((int)Math.Log(bytes, 1024), _units.Length - 1);
        var value = bytes / Math.Pow(1024, order);
        return $"{value.ToString($"F{decimals}")} {_units[order]}";
    }

    public static string FormatNumber(uint n)
    {
        return n.ToString("N0", new System.Globalization.CultureInfo("pt-BR"));
    }
}