using DiskIntelligence.Helpers;
using DiskIntelligence.Services;
using DiskIntelligence.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DiskIntelligence.Pages;

public sealed partial class AdvancedSearchPage : Page
{
    public MainViewModel ViewModel { get; }
    private StackPanel _contentStack = null!;
    private TextBox _queryBox = null!;
    private ComboBox _minSizeBox = null!;
    private TextBox _extensionBox = null!;
    private StackPanel _resultsArea = null!;

    public AdvancedSearchPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        _contentStack = new StackPanel { Padding = new Thickness(24), Spacing = 16 };
        RootGrid.Children.Add(new ScrollViewer { Content = _contentStack });
        BuildUI();
    }

    private void BuildUI()
    {
        _contentStack.Children.Add(new TextBlock { Text = "Busca Avancada", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] });

        var filterCard = new Border
        {
            Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Padding = new Thickness(16),
        };
        var filterPanel = new StackPanel { Spacing = 12 };

        var filterGrid = new Grid { ColumnSpacing = 12 };
        filterGrid.ColumnDefinitions.Add(new ColumnDefinition());
        filterGrid.ColumnDefinitions.Add(new ColumnDefinition());
        filterGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var queryPanel = new StackPanel { Spacing = 4 };
        queryPanel.Children.Add(new TextBlock { Text = "Nome do Arquivo", FontSize = 12, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        _queryBox = new TextBox { PlaceholderText = "Ex: relatorio, backup..." };
        queryPanel.Children.Add(_queryBox);
        Grid.SetColumn(queryPanel, 0);
        filterGrid.Children.Add(queryPanel);

        var sizePanel = new StackPanel { Spacing = 4 };
        sizePanel.Children.Add(new TextBlock { Text = "Tamanho Minimo", FontSize = 12, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        _minSizeBox = new ComboBox();
        _minSizeBox.Items.Add(new ComboBoxItem { Content = "Qualquer tamanho", Tag = 0UL });
        _minSizeBox.Items.Add(new ComboBoxItem { Content = "1 MB", Tag = 1048576UL });
        _minSizeBox.Items.Add(new ComboBoxItem { Content = "10 MB", Tag = 10485760UL });
        _minSizeBox.Items.Add(new ComboBoxItem { Content = "100 MB", Tag = 104857600UL });
        _minSizeBox.Items.Add(new ComboBoxItem { Content = "1 GB", Tag = 1073741824UL });
        _minSizeBox.SelectedIndex = 2;
        sizePanel.Children.Add(_minSizeBox);
        Grid.SetColumn(sizePanel, 1);
        filterGrid.Children.Add(sizePanel);

        var extPanel = new StackPanel { Spacing = 4 };
        extPanel.Children.Add(new TextBlock { Text = "Extensao", FontSize = 12, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        _extensionBox = new TextBox { PlaceholderText = "Ex: mp4, pdf, zip..." };
        extPanel.Children.Add(_extensionBox);
        Grid.SetColumn(extPanel, 2);
        filterGrid.Children.Add(extPanel);

        filterPanel.Children.Add(filterGrid);

        var searchBtn = new Button { Content = "Buscar Arquivos", HorizontalAlignment = HorizontalAlignment.Stretch };
        searchBtn.Click += OnSearch;
        filterPanel.Children.Add(searchBtn);

        filterCard.Child = filterPanel;
        _contentStack.Children.Add(filterCard);

        _resultsArea = new StackPanel();
    }

    private void OnSearch(object sender, RoutedEventArgs e)
    {
        _contentStack.Children.Remove(_resultsArea);
        _resultsArea = new StackPanel { Spacing = 8 };

        if (ViewModel.ScanResult == null) return;

        var query = _queryBox.Text;
        var minSize = ((_minSizeBox.SelectedItem as ComboBoxItem)?.Tag as ulong?) ?? 0UL;
        var ext = _extensionBox.Text.TrimStart('.');

        var results = FileOperationsService.SearchFiles(ViewModel.ScanResult.ScanPath, query, minSize, ext);

        _resultsArea.Children.Add(new TextBlock { Text = $"Resultados ({results.Count}{(results.Count >= 100 ? "+" : "")})", Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"], Margin = new Thickness(0, 8, 0, 0) });

        if (results.Count == 0)
        {
            _resultsArea.Children.Add(new TextBlock { Text = "Nenhum arquivo encontrado para os filtros atuais.", Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        }
        else
        {
            foreach (var f in results)
            {
                var row = new Grid { Padding = new Thickness(8, 4, 8, 4), ColumnSpacing = 12 };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var info = new StackPanel();
                info.Children.Add(new TextBlock { Text = Truncate(f.Name, 40), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis });
                info.Children.Add(new TextBlock { Text = Truncate(f.Path, 50), FontSize = 11, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"], TextTrimming = TextTrimming.CharacterEllipsis });
                Grid.SetColumn(info, 0);
                row.Children.Add(info);

                var badge = new Border { Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"], CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 2, 6, 2) };
                badge.Child = new TextBlock { Text = f.Extension, FontSize = 11 };
                Grid.SetColumn(badge, 1);
                row.Children.Add(badge);

                var sizeLabel = new TextBlock { Text = ByteFormatter.Format(f.Size), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"], VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(sizeLabel, 2);
                row.Children.Add(sizeLabel);

                var delBtn = new Button { Content = "Excluir", FontSize = 11, Padding = new Thickness(8, 4, 8, 4) };
                var p = f.Path;
                delBtn.Click += (_, _) => ViewModel.DeleteFileCommand.Execute(p);
                Grid.SetColumn(delBtn, 3);
                row.Children.Add(delBtn);

                _resultsArea.Children.Add(row);
                _resultsArea.Children.Add(new Border { Height = 1, Background = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"] });
            }
        }

        _contentStack.Children.Add(_resultsArea);
    }

    private static string Truncate(string s, int len) => s.Length <= len ? s : "..." + s[^len..];
}