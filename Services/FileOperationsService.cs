using DiskIntelligence.Helpers;
using DiskIntelligence.Models;

namespace DiskIntelligence.Services;

public static class FileOperationsService
{
    private static readonly string[] CriticalPaths =
    [
        @"C:\Windows",
        @"C:\Program Files",
        @"C:\Program Files (x86)",
        @"C:\ProgramData",
        @"C:\Users\Default",
    ];

    public static bool IsSafeToDelete(string path)
    {
        var lower = path.ToLowerInvariant();

        foreach (var cp in CriticalPaths)
        {
            if (lower.StartsWith(cp.ToLowerInvariant()))
                return false;
        }

        if (lower.Length <= 3 && lower.EndsWith(@":\"))
            return false;

        return true;
    }

    public static void DeleteFile(string path)
    {
        if (!IsSafeToDelete(path))
            throw new UnauthorizedAccessException($"Acesso negado: O caminho {path} e critico e protegido pelo sistema.");

        if (!File.Exists(path))
            throw new FileNotFoundException("Arquivo nao encontrado.");

        File.Delete(path);
    }

    public static List<BigFile> SearchFiles(string rootPath, string query, ulong minSize, string extension)
    {
        var results = new List<BigFile>();
        var q = query.ToLowerInvariant();
        var ext = extension.ToLowerInvariant().TrimStart('.');

        try
        {
            foreach (var filePath in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    var fi = new FileInfo(filePath);
                    var size = (ulong)fi.Length;
                    if (size < minSize) continue;

                    var fileName = fi.Name;
                    var fileExt = fi.Extension.ToLowerInvariant().TrimStart('.');

                    var matchesQuery = string.IsNullOrEmpty(q) || fileName.ToLowerInvariant().Contains(q);
                    var matchesExt = string.IsNullOrEmpty(ext) || fileExt == ext;

                    if (matchesQuery && matchesExt)
                    {
                        results.Add(new BigFile
                        {
                            Path = filePath,
                            Name = fileName,
                            Size = size,
                            Extension = fileExt,
                            Category = FileClassifier.Classify(fileExt),
                        });

                        if (results.Count >= 100) break;
                    }
                }
                catch { }
            }
        }
        catch { }

        results.Sort((a, b) => b.Size.CompareTo(a.Size));
        return results;
    }
}