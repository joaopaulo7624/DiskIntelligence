using DiskIntelligence.Helpers;
using DiskIntelligence.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DiskIntelligence.Pages;

public sealed partial class FilesPage : Page
{
    public MainViewModel ViewModel { get; }
    private StackPanel _contentStack = null!;

    public FilesPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        _contentStack = new StackPanel { Padding = new Thickness(24), Spacing = 16 };
        RootGrid.Children.Add(new ScrollViewer { Content = _contentStack });
        ViewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ViewModel.ScanResult)) RefreshFiles(); };
        BuildUI();
    }

    private void BuildUI()
    {
        _contentStack.Children.Add(new TextBlock { Text = "Maiores Arquivos", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] });
        _contentStack.Children.Add(new TextBlock { Text = "Top 20", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"], Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        RefreshFiles();

        _contentStack.Children.Add(new TextBlock { Text = "Top Extensoes", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"], Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"], Margin = new Thickness(0, 16, 0, 0) });
        RefreshExtensions();
    }

    private void RefreshFiles()
    {
        var topFiles = ViewModel.ScanResult?.TopFiles;
        if (topFiles == null || topFiles.Count == 0) return;

        var stack = new StackPanel();
        foreach (var f in topFiles)
        {
            var grid = new Grid { Padding = new Thickness(8, 4, 8, 4), ColumnSpacing = 12 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var namePanel = new StackPanel();
            namePanel.Children.Add(new TextBlock { Text = Truncate(f.Name, 50), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis });
            namePanel.Children.Add(new TextBlock { Text = Truncate(f.Path, 60), FontSize = 11, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"], TextTrimming = TextTrimming.CharacterEllipsis });
            Grid.SetColumn(namePanel, 0);
            grid.Children.Add(namePanel);

            var extBadge = new Border { Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"], CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 2, 6, 2) };
            extBadge.Child = new TextBlock { Text = f.Extension, FontSize = 11 };
            Grid.SetColumn(extBadge, 1);
            grid.Children.Add(extBadge);

            var sizeLabel = new TextBlock { Text = ByteFormatter.Format(f.Size), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"], VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(sizeLabel, 2);
            grid.Children.Add(sizeLabel);

            var delBtn = new Button { Content = "Excluir", FontSize = 11, Padding = new Thickness(8, 4, 8, 4) };
            var path = f.Path;
            delBtn.Click += async (_, _) =>
            {
                if (await ConfirmDelete(f.Path, f.Name))
                {
                    ViewModel.DeleteFileCommand.Execute(f.Path);
                }
            };
            Grid.SetColumn(delBtn, 3);
            grid.Children.Add(delBtn);

            var separator = new Border { Height = 1, Background = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"] };
            stack.Children.Add(grid);
            stack.Children.Add(separator);
        }
        _contentStack.Children.Add(stack);
    }

    private void RefreshExtensions()
    {
        var topExt = ViewModel.ScanResult?.TopExtensions;
        if (topExt == null || topExt.Count == 0) return;

        var wrap = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        foreach (var ext in topExt)
        {
            var card = new Border
            {
                Width = 180, Padding = new Thickness(14), Margin = new Thickness(4),
                CornerRadius = new CornerRadius(8),
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1),
            };
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = $".{ext.Extension}", FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"] });
            panel.Children.Add(new TextBlock { Text = $"{ByteFormatter.FormatNumber(ext.Count)} arquivos", FontSize = 11, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"], Margin = new Thickness(0, 2, 0, 0) });
            panel.Children.Add(new TextBlock { Text = ByteFormatter.Format(ext.Size), FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 2, 0, 0) });
            card.Child = panel;
            wrap.Children.Add(card);
        }
        _contentStack.Children.Add(wrap);
    }

    private static async Task<bool> ConfirmDelete(string path, string name)
    {
        var dlg = new ContentDialog
        {
            Title = "Confirmar Exclusao",
            Content = $"Deseja realmente apagar?\n{name}",
            PrimaryButtonText = "Excluir",
            CloseButtonText = "Cancelar",
            XamlRoot = App.MainWindow?.Content?.XamlRoot,
        };
        var result = await dlg.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private static string Truncate(string s, int len) => s.Length <= len ? s : "..." + s[^len..];
}