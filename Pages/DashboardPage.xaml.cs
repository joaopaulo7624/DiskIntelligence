using DiskIntelligence.Helpers;
using DiskIntelligence.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;

namespace DiskIntelligence.Pages;

public sealed partial class DashboardPage : Page
{
    public MainViewModel ViewModel { get; }
    private StackPanel _contentStack = null!;
    private readonly List<(TextBlock, string)> _statBlocks = [];

    public DashboardPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        _contentStack = new StackPanel { Padding = new Thickness(24), Spacing = 16 };
        RootGrid.Children.Add(new ScrollViewer { Content = _contentStack });
        BuildUI();
        ViewModel.PropertyChanged += (_, _) => RefreshStats();
    }

    private void BuildUI()
    {
        _contentStack.Children.Add(MakeTitle("Visao Geral"));

        var statsGrid = new Grid { ColumnSpacing = 12 };
        for (int i = 0; i < 4; i++) statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _contentStack.Children.Add(statsGrid);

        AddStat(statsGrid, 0, "ESPACO OCUPADO", () => ViewModel.ScanResult?.TotalSize != null ? ByteFormatter.Format(ViewModel.ScanResult.TotalSize) : "—");
        AddStat(statsGrid, 1, "TOTAL ARQUIVOS", () => ViewModel.ScanResult != null ? ByteFormatter.FormatNumber(ViewModel.ScanResult.TotalFiles) : "—");
        AddStat(statsGrid, 2, "PASTAS VAZIAS", () => ViewModel.ScanResult != null ? ViewModel.ScanResult.EmptyDirsCount.ToString() : "—");
        AddStat(statsGrid, 3, "TEMPO ANALISE", () => ViewModel.ScanResult != null ? $"{ViewModel.ScanResult.DurationMs} ms" : "—");

        _contentStack.Children.Add(CreateDistributionCard());
        _contentStack.Children.Add(CreateCategoriesCard());
    }

    private void AddStat(Grid grid, int col, string title, Func<string> getValue)
    {
        var card = new Border
        {
            Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
        };
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = title, Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"], Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        var val = new TextBlock { FontSize = 22, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 6, 0, 0), Text = "—" };
        _statBlocks.Add((val, title));
        panel.Children.Add(val);
        Grid.SetColumn(card, col);
        card.Child = panel;
        grid.Children.Add(card);
    }

    private void RefreshStats()
    {
        foreach (var (block, title) in _statBlocks)
        {
            block.Text = title switch
            {
                "ESPACO OCUPADO" => ViewModel.ScanResult?.TotalSize != null ? ByteFormatter.Format(ViewModel.ScanResult.TotalSize) : "—",
                "TOTAL ARQUIVOS" => ViewModel.ScanResult != null ? ByteFormatter.FormatNumber(ViewModel.ScanResult.TotalFiles) : "—",
                "PASTAS VAZIAS" => ViewModel.ScanResult != null ? ViewModel.ScanResult.EmptyDirsCount.ToString() : "—",
                "TEMPO ANALISE" => ViewModel.ScanResult != null ? $"{ViewModel.ScanResult.DurationMs} ms" : "—",
                _ => "—"
            };
        }
    }

    private FrameworkElement CreateDistributionCard()
    {
        var card = CardBorder();
        var panel = new StackPanel();
        panel.Children.Add(CardTitle("DISTRIBUICAO POR TAMANHO"));
        var r = ViewModel.ScanResult;
        var tags = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 8, 0, 0) };
        AddTag(tags, "#22c55e", "0-10 KB", r?.TinyFiles ?? 0);
        AddTag(tags, "#3b82f6", "10 KB-1 MB", r?.SmallFiles ?? 0);
        AddTag(tags, "#f59e0b", "1-100 MB", r?.MediumFiles ?? 0);
        AddTag(tags, "#f97316", "100 MB-1 GB", r?.LargeFiles ?? 0);
        AddTag(tags, "#ef4444", "1 GB+", r?.HugeFiles ?? 0);
        panel.Children.Add(tags);
        card.Child = panel;
        return card;
    }

    private void AddTag(StackPanel parent, string color, string label, uint count)
    {
        var tag = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        tag.Children.Add(new Border { Width = 10, Height = 10, Background = ParseColor(color), CornerRadius = new CornerRadius(2) });
        tag.Children.Add(new TextBlock { Text = $"{label} ({ByteFormatter.FormatNumber(count)})", FontSize = 11, Foreground = ParseColor(color) });
        parent.Children.Add(tag);
    }

    private FrameworkElement CreateCategoriesCard()
    {
        var card = CardBorder();
        var panel = new StackPanel();
        panel.Children.Add(CardTitle("CATEGORIAS POR OCUPACAO"));

        var list = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        var cats = ViewModel.ScanResult?.Categories;
        if (cats != null)
        {
            foreach (var cat in cats)
            {
                var row = new Grid { Height = 36, Margin = new Thickness(0, 2, 0, 2) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
                row.ColumnDefinitions.Add(new ColumnDefinition());

                var name = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, Text = $"{cat.Icon} {cat.Name}" };
                Grid.SetColumn(name, 0);
                row.Children.Add(name);

                var barGrid = new Grid { Margin = new Thickness(8, 0, 0, 0) };
                barGrid.Children.Add(new Border { Background = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"], CornerRadius = new CornerRadius(4) });
                var fill = new Border { CornerRadius = new CornerRadius(4), HorizontalAlignment = HorizontalAlignment.Left, MinWidth = 4, Width = cat.Percentage, Background = ParseColor(cat.Color) };
                barGrid.Children.Add(fill);
                Grid.SetColumn(barGrid, 1);
                row.Children.Add(barGrid);

                list.Children.Add(row);
            }
        }
        panel.Children.Add(list);
        card.Child = panel;
        return card;
    }

    private static Border CardBorder() => new()
    {
        Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
        BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(8),
        Padding = new Thickness(16),
    };

    private static TextBlock MakeTitle(string t) => new() { Text = t, Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] };
    private static TextBlock CardTitle(string t) => new() { Text = t, Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"], Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] };

    private static SolidColorBrush ParseColor(string hex)
    {
        try { return new SolidColorBrush(ColorHelper.FromArgb(255, byte.Parse(hex[1..3], System.Globalization.NumberStyles.HexNumber), byte.Parse(hex[3..5], System.Globalization.NumberStyles.HexNumber), byte.Parse(hex[5..7], System.Globalization.NumberStyles.HexNumber))); }
        catch { return new SolidColorBrush(Colors.Gray); }
    }
}