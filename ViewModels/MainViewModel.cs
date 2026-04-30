using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskIntelligence.Helpers;
using DiskIntelligence.Models;
using DiskIntelligence.Services;

namespace DiskIntelligence.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private string _scanStatus = "Pronto";
    [ObservableProperty] private string _currentPath = "";
    [ObservableProperty] private uint _filesScanned;
    [ObservableProperty] private uint _dirsScanned;
    [ObservableProperty] private ulong _totalSizeScanned;
    [ObservableProperty] private string _scanPath = "";

    [ObservableProperty] private ScanResult? _scanResult;
    [ObservableProperty] private ObservableCollection<InstalledSoftware> _installedSoftware = [];

    private CancellationTokenSource? _cts;

    // Command bindings
    [RelayCommand]
    private void CancelScan() => _cts?.Cancel();

    [RelayCommand]
    private async Task StartScan()
    {
        IsScanning = true;
        ScanStatus = "Realizando analise...";
        FilesScanned = 0;
        DirsScanned = 0;
        TotalSizeScanned = 0;
        ScanResult = null;

        _cts = new CancellationTokenSource();
        var progress = new Progress<ScanProgress>(p =>
        {
            CurrentPath = p.CurrentPath;
            FilesScanned = p.FilesScanned;
            DirsScanned = p.DirsScanned;
            TotalSizeScanned = p.TotalSize;
        });

        try
        {
            var result = await Task.Run(() =>
            {
                var scanner = new DiskScannerService(_cts.Token, progress);
                return scanner.Scan(ScanPath);
            }, _cts.Token);

            ScanResult = result;
            ScanStatus = $"Analise concluida em {result.DurationMs}ms";
        }
        catch (OperationCanceledException)
        {
            ScanStatus = "Analise cancelada.";
        }
        catch (Exception ex)
        {
            ScanStatus = $"Erro: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private async Task SelectFolder()
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            ScanPath = folder.Path;
            await StartScan();
        }
    }

    [RelayCommand]
    private void DeleteFile(string path)
    {
        try
        {
            FileOperationsService.DeleteFile(path);
        }
        catch (Exception ex)
        {
            ScanStatus = $"Erro ao excluir: {ex.Message}";
        }
    }

    public List<BigFile> SearchFiles(string query, ulong minSize, string extension)
    {
        if (ScanResult == null) return [];
        return FileOperationsService.SearchFiles(ScanResult.ScanPath, query, minSize, extension);
    }

    [RelayCommand]
    private void LoadSoftware()
    {
        try
        {
            var software = SoftwareEnumeratorService.GetInstalledSoftware();
            InstalledSoftware = new ObservableCollection<InstalledSoftware>(software);
        }
        catch (Exception ex)
        {
            ScanStatus = $"Erro ao carregar software: {ex.Message}";
            InstalledSoftware.Clear();
        }
    }

    public string ScanSize => ByteFormatter.Format(TotalSizeScanned);
    public string FilesScannedText => ByteFormatter.FormatNumber(FilesScanned);
    public string DirsScannedText => ByteFormatter.FormatNumber(DirsScanned);
}