using DiskIntelligence.Models;
using Microsoft.Win32;

namespace DiskIntelligence.Services;

public static class SoftwareEnumeratorService
{
    public static List<InstalledSoftware> GetInstalledSoftware()
    {
        var list = new List<InstalledSoftware>();

        var paths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
        };

        foreach (var path in paths)
        {
            EnumerateFromKey(Registry.LocalMachine, path, list);
        }

        EnumerateFromKey(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Uninstall", list);

        return list.OrderByDescending(s => s.EstimatedSize).ToList();
    }

    private static void EnumerateFromKey(RegistryKey root, string path, List<InstalledSoftware> list)
    {
        try
        {
            using var key = root.OpenSubKey(path);
            if (key == null) return;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null) continue;

                var name = subKey.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(name)) continue;

                list.Add(new InstalledSoftware
                {
                    Name = name,
                    Version = subKey.GetValue("DisplayVersion") as string ?? "",
                    Publisher = subKey.GetValue("Publisher") as string ?? "",
                    InstallDate = subKey.GetValue("InstallDate") as string ?? "",
                    EstimatedSize = ((uint)(subKey.GetValue("EstimatedSize") ?? 0)) * 1024UL,
                });
            }
        }
        catch { }
    }
}