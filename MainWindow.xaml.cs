using DiskIntelligence.Pages;
using DiskIntelligence.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace DiskIntelligence;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private Frame _contentFrame = null!;
    private ProgressBar _progressBar = null!;
    private TextBlock _statusText = null!;
    private TextBlock _progressPath = null!;
    private TextBlock _progressStats = null!;
    private StackPanel _progressPanel = null!;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        _viewModel.PropertyChanged += OnViewModelChanged;
        BuildUI();
        App.MainWindow = this;
    }

    private void BuildUI()
    {
        var nav = new NavigationView
        {
            PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact,
            IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed,
            IsSettingsVisible = false,
            CompactPaneLength = 48,
            OpenPaneLength = 220,
        };

        nav.MenuItems.Add(CreateNavItem("Visao Geral", "\U0001f4ca", "dashboard"));
        nav.MenuItems.Add(CreateNavItem("Maiores Arquivos", "\U0001f4c8", "files"));
        nav.MenuItems.Add(CreateNavItem("Duplicados", "\U0001f4cb", "duplicates"));
        nav.MenuItems.Add(CreateNavItem("Programas Instalados", "\U0001f4e6", "software"));
        nav.MenuItems.Add(CreateNavItem("Busca Avancada", "\U0001f50d", "advanced"));

        var scanBtn = new Button
        {
            Content = "Nova Analise",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(8, 4, 8, 8),
        };
        scanBtn.SetBinding(Button.CommandProperty, new Microsoft.UI.Xaml.Data.Binding { Source = _viewModel, Path = new PropertyPath("SelectFolderCommand") });

        var cancelBtn = new Button
        {
            Content = "Cancelar",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(8, 0, 8, 8),
            Visibility = Visibility.Collapsed,
        };
        cancelBtn.SetBinding(Button.CommandProperty, new Microsoft.UI.Xaml.Data.Binding { Source = _viewModel, Path = new PropertyPath("CancelScanCommand") });

        _contentFrame = new Frame();

        _progressPanel = new StackPanel { Padding = new Thickness(16), Spacing = 8, Visibility = Visibility.Collapsed };
        _progressBar = new ProgressBar { IsIndeterminate = true };
        _statusText = new TextBlock { Text = "Pronto", FontSize = 12, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] };
        _progressStats = new TextBlock { FontSize = 11, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] };
        _progressPath = new TextBlock
        {
            FontSize = 10,
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontFamily = new FontFamily("Cascadia Code, Consolas, monospace"),
        };

        _progressPanel.Children.Add(_statusText);
        _progressPanel.Children.Add(_progressBar);
        _progressPanel.Children.Add(_progressStats);
        _progressPanel.Children.Add(_progressPath);
        _progressPanel.Children.Add(cancelBtn);

        var footerStack = new StackPanel();
        footerStack.Children.Add(_progressPanel);
        footerStack.Children.Add(scanBtn);
        nav.PaneFooter = footerStack;

        nav.ItemInvoked += (s, e) =>
        {
            var tag = e.InvokedItemContainer?.Tag?.ToString();
            NavigateTo(tag);
        };

        nav.Content = _contentFrame;
        RootGrid.Children.Add(nav);

        NavigateTo("dashboard");
    }

    private static NavigationViewItem CreateNavItem(string text, string icon, string tag)
    {
        var item = new NavigationViewItem
        {
            Tag = tag,
        };

        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = icon, FontSize = 16, VerticalAlignment = VerticalAlignment.Center });
        panel.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });
        item.Content = panel;

        return item;
    }

    private void NavigateTo(string? tag)
    {
        Page page = tag switch
        {
            "files" => new FilesPage(_viewModel),
            "duplicates" => new DuplicatesPage(_viewModel),
            "software" => new SoftwarePage(_viewModel),
            "advanced" => new AdvancedSearchPage(_viewModel),
            _ => new DashboardPage(_viewModel),
        };
        _contentFrame.Content = page;
    }

    private void OnViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.IsScanning):
                _progressBar.IsIndeterminate = _viewModel.IsScanning;
                _progressPanel.Visibility = _viewModel.IsScanning ? Visibility.Visible : Visibility.Collapsed;
                break;
            case nameof(MainViewModel.ScanStatus):
                _statusText.Text = _viewModel.ScanStatus;
                break;
            case nameof(MainViewModel.CurrentPath):
                _progressPath.Text = _viewModel.CurrentPath;
                break;
            case nameof(MainViewModel.FilesScanned):
            case nameof(MainViewModel.DirsScanned):
            case nameof(MainViewModel.TotalSizeScanned):
                _progressStats.Text = $"{Helpers.ByteFormatter.FormatNumber(_viewModel.FilesScanned)} arquivos | {Helpers.ByteFormatter.Format(_viewModel.TotalSizeScanned)}";
                break;
            case nameof(MainViewModel.ScanResult):
                if (_viewModel.ScanResult != null)
                    _statusText.Text = $"Analise concluida em {_viewModel.ScanResult.DurationMs}ms | {Helpers.ByteFormatter.FormatNumber(_viewModel.ScanResult.TotalFiles)} arquivos";
                break;
        }
    }
}