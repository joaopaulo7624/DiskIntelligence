using System.Collections.Concurrent;
using System.Security.Cryptography;
using DiskIntelligence.Helpers;
using DiskIntelligence.Models;

namespace DiskIntelligence.Services;

public class DiskScannerService(CancellationToken token, IProgress<ScanProgress> progress)
{
    private readonly CancellationToken _token = token;
    private readonly IProgress<ScanProgress> _progress = progress;
    private readonly PriorityQueue<BigFile, ulong> _topFiles = new();
    private readonly ConcurrentDictionary<string, ulong> _catSizes = new();
    private readonly ConcurrentDictionary<string, uint> _catCounts = new();
    private readonly ConcurrentDictionary<string, ulong> _extSizes = new();
    private readonly ConcurrentDictionary<string, uint> _extCounts = new();
    private readonly ConcurrentDictionary<ulong, ConcurrentBag<string>> _sizeToPaths = new();
    private readonly ConcurrentBag<string> _emptyDirs = [];

    private ulong _totalSize;
    private uint _totalFiles;
    private uint _totalDirs;
    private uint _tinyFiles;
    private uint _smallFiles;
    private uint _mediumFiles;
    private uint _largeFiles;
    private uint _hugeFiles;
    private int _maxDepth;
    private string _deepestPath = "";
    private string _oldestFile = "";
    private string _newestFile = "";
    private long _oldestSecs = long.MaxValue;
    private long _newestSecs;

    public ScanResult Scan(string rootPath)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var baseDepth = Path.GetFullPath(rootPath).Count(c => c == Path.DirectorySeparatorChar);
        var lastEmit = Environment.TickCount64;

        var dirs = new ConcurrentQueue<string>();
        dirs.Enqueue(rootPath);

        while (dirs.TryDequeue(out var dir))
        {
            _token.ThrowIfCancellationRequested();
            ProcessDirectory(dir, baseDepth, dirs, ref lastEmit);
        }

        EmitProgress("", _totalFiles, _totalDirs, _totalSize, ref lastEmit);

        var result = new ScanResult
        {
            TotalSize = _totalSize,
            TotalFiles = _totalFiles,
            TotalDirs = _totalDirs,
            DurationMs = sw.ElapsedMilliseconds,
            ScanPath = rootPath,
            TinyFiles = _tinyFiles,
            SmallFiles = _smallFiles,
            MediumFiles = _mediumFiles,
            LargeFiles = _largeFiles,
            HugeFiles = _hugeFiles,
            MaxDepth = _maxDepth,
            DeepestPath = _deepestPath,
            OldestFile = _oldestFile,
            NewestFile = _newestFile,
            EmptyDirs = _emptyDirs.ToList(),
            EmptyDirsCount = (uint)_emptyDirs.Count,
        };

        result = BuildCategories(result);
        result = BuildTopFiles(result);
        result = BuildExtensions(result);
        result = BuildDuplicates(result);

        sw.Stop();
        result = result with { DurationMs = sw.ElapsedMilliseconds };

        return result;
    }

    private void ProcessDirectory(string dir, int baseDepth, ConcurrentQueue<string> dirs, ref long lastEmit)
    {
        try
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(dir))
            {
                _token.ThrowIfCancellationRequested();

                FileAttributes attr;
                try { attr = File.GetAttributes(entry); }
                catch { continue; }

                if ((attr & FileAttributes.Directory) != 0)
                {
                    Interlocked.Increment(ref _totalDirs);
                    var depth = entry.Count(c => c == Path.DirectorySeparatorChar) - baseDepth;
                    if (depth > _maxDepth)
                    {
                        _maxDepth = depth;
                        _deepestPath = entry;
                    }

                    bool isEmpty = true;
                    try
                    {
                        using var enumerator = Directory.EnumerateFileSystemEntries(entry).GetEnumerator();
                        isEmpty = !enumerator.MoveNext();
                    }
                    catch { }

                    if (isEmpty && _emptyDirs.Count < 50)
                        _emptyDirs.Add(entry);

                    dirs.Enqueue(entry);
                }
                else
                {
                    ProcessFile(entry);
                }

                if (Environment.TickCount64 - lastEmit > 100)
                    EmitProgress(entry, _totalFiles, _totalDirs, _totalSize, ref lastEmit);
            }
        }
        catch { }
    }

    private void ProcessFile(string path)
    {
        Interlocked.Increment(ref _totalFiles);

        try
        {
            var fi = new FileInfo(path);
            var size = (ulong)fi.Length;
            Interlocked.Add(ref _totalSize, size);

            if (size <= 10_239UL) Interlocked.Increment(ref _tinyFiles);
            else if (size <= 1_048_575UL) Interlocked.Increment(ref _smallFiles);
            else if (size <= 104_857_599UL) Interlocked.Increment(ref _mediumFiles);
            else if (size <= 1_073_741_823UL) Interlocked.Increment(ref _largeFiles);
            else Interlocked.Increment(ref _hugeFiles);

            var ext = Path.GetExtension(path).ToLowerInvariant().TrimStart('.');
            var cat = FileClassifier.Classify(ext);

            _catSizes.AddOrUpdate(cat, size, (_, v) => v + size);
            _catCounts.AddOrUpdate(cat, 1, (_, v) => v + 1);

            var extKey = string.IsNullOrEmpty(ext) ? "(sem extensao)" : ext;
            _extSizes.AddOrUpdate(extKey, size, (_, v) => v + size);
            _extCounts.AddOrUpdate(extKey, 1, (_, v) => v + 1);

            if (size >= 10_240)
                _sizeToPaths.GetOrAdd(size, _ => []).Add(path);

            var bf = new BigFile { Path = path, Name = Path.GetFileName(path), Size = size, Extension = ext, Category = cat };
            lock (_topFiles)
            {
                if (_topFiles.Count < 20)
                    _topFiles.Enqueue(bf, size);
                else
                    _topFiles.EnqueueDequeue(bf, size);
            }

            var writeTime = fi.LastWriteTimeUtc;
            var secs = ((DateTimeOffset)writeTime).ToUnixTimeSeconds();
            lock (_topFiles)
            {
                if (secs < _oldestSecs) { _oldestSecs = secs; _oldestFile = path; }
                if (secs > _newestSecs) { _newestSecs = secs; _newestFile = path; }
            }
        }
        catch { }
    }

    private void EmitProgress(string path, uint files, uint dirs, ulong size, ref long lastEmit)
    {
        _progress.Report(new ScanProgress { CurrentPath = path, FilesScanned = files, DirsScanned = dirs, TotalSize = size });
        lastEmit = Environment.TickCount64;
    }

    private ScanResult BuildCategories(ScanResult result)
    {
        var categories = _catSizes.Select(kvp =>
        {
            var (color, icon) = FileClassifier.GetCategoryMeta(kvp.Key);
            var count = _catCounts.GetValueOrDefault(kvp.Key);
            var pct = result.TotalSize > 0 ? (kvp.Value / (double)result.TotalSize) * 100.0 : 0;
            return new CategoryInfo { Name = kvp.Key, Size = kvp.Value, Count = count, Color = color, Icon = icon, Percentage = pct };
        }).OrderByDescending(c => c.Size).ToList();

        return result with { Categories = categories };
    }

    private ScanResult BuildTopFiles(ScanResult result)
    {
        var top = new List<BigFile>();
        lock (_topFiles)
        {
            while (_topFiles.Count > 0) top.Add(_topFiles.Dequeue());
        }

        top.Sort((a, b) => b.Size.CompareTo(a.Size));
        return result with { TopFiles = top };
    }

    private ScanResult BuildExtensions(ScanResult result)
    {
        var exts = _extSizes.Select(kvp =>
        {
            var count = _extCounts.GetValueOrDefault(kvp.Key);
            return new ExtensionStat { Extension = kvp.Key, Size = kvp.Value, Count = count };
        }).OrderByDescending(e => e.Size).Take(15).ToList();

        return result with { TopExtensions = exts };
    }

    private ScanResult BuildDuplicates(ScanResult result)
    {
        _progress.Report(new ScanProgress { CurrentPath = "Analisando duplicados...", FilesScanned = result.TotalFiles, DirsScanned = result.TotalDirs, TotalSize = result.TotalSize });

        var dupGroups = new ConcurrentBag<DuplicateGroup>();

        var sizeGroups = _sizeToPaths.Where(kvp => kvp.Value.Count > 1).ToList();
        Parallel.ForEach(sizeGroups, kvp =>
        {
            _token.ThrowIfCancellationRequested();
            var hashMap = new ConcurrentDictionary<string, ConcurrentBag<string>>();

            Parallel.ForEach(kvp.Value, path =>
            {
                var hash = ComputeSha256(path);
                if (hash != null)
                    hashMap.GetOrAdd(hash, _ => []).Add(path);
            });

            foreach (var (_, paths) in hashMap.Where(h => h.Value.Count > 1))
            {
                var pathList = paths.ToList();
                var wasted = kvp.Key * ((ulong)pathList.Count - 1);
                dupGroups.Add(new DuplicateGroup { Size = kvp.Key, Paths = pathList, WastedBytes = wasted });
            }
        });

        var sorted = dupGroups.OrderByDescending(g => g.WastedBytes).Take(100).ToList();
        ulong totalWasted = 0;
        foreach (var g in sorted) totalWasted += g.WastedBytes;

        return result with { PotentialDuplicates = sorted, PotentialWasted = totalWasted };
    }

    private static string? ComputeSha256(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var hash = SHA256.HashData(stream);
            return Convert.ToHexStringLower(hash);
        }
        catch
        {
            return null;
        }
    }
}