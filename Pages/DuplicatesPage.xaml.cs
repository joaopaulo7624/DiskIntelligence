using DiskIntelligence.Helpers;
using DiskIntelligence.Models;
using DiskIntelligence.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DiskIntelligence.Pages;

public sealed partial class DuplicatesPage : Page
{
    public MainViewModel ViewModel { get; }
    private StackPanel _contentStack = null!;

    public DuplicatesPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        _contentStack = new StackPanel { Padding = new Thickness(24), Spacing = 16 };
        RootGrid.Children.Add(new ScrollViewer { Content = _contentStack });
        ViewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ViewModel.ScanResult)) Refresh(); };
        BuildUI();
    }

    private void BuildUI()
    {
        _contentStack.Children.Add(new TextBlock { Text = "Arquivos Duplicados", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] });
        Refresh();
    }

    private void Refresh()
    {
        _contentStack.Children.Clear();
        _contentStack.Children.Add(new TextBlock { Text = "Arquivos Duplicados", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] });

        var dups = ViewModel.ScanResult?.PotentialDuplicates;
        if (dups == null || dups.Count == 0)
        {
            _contentStack.Children.Add(new TextBlock { Text = "Nenhum Arquivo Duplicado Encontrado", FontSize = 16, Foreground = new SolidColorBrush(Colors.Green) });
            _contentStack.Children.Add(new TextBlock { Text = "Seu disco esta organizado.", FontSize = 13, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
            return;
        }

        var warning = new Border
        {
            Background = (Brush)Application.Current.Resources["SystemFillColorCautionBackgroundBrush"],
            BorderBrush = (Brush)Application.Current.Resources["SystemFillColorCautionBrush"],
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Padding = new Thickness(16),
        };
        var warnPanel = new StackPanel { Spacing = 4 };
        warnPanel.Children.Add(new TextBlock { Text = "Duplicatas Detectadas", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        warnPanel.Children.Add(new TextBlock { Text = "Arquivos com conteudo identico identificados via SHA-256", FontSize = 12, Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
        warnPanel.Children.Add(new TextBlock { Text = $"Economia potencial: {ByteFormatter.Format(ViewModel.ScanResult!.PotentialWasted)}", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"] });
        warning.Child = warnPanel;
        _contentStack.Children.Add(warning);

        foreach (var g in dups)
        {
            var card = new Border
            {
                Margin = new Thickness(0, 2, 0, 2), Padding = new Thickness(14), CornerRadius = new CornerRadius(8),
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1),
            };
            var panel = new StackPanel { Spacing = 4 };

            var headerRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            headerRow.Children.Add(new TextBlock { Text = ByteFormatter.Format(g.Size), FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"] });
            headerRow.Children.Add(new TextBlock { Text = $"x {g.Paths.Count} copias", Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
            headerRow.Children.Add(new TextBlock { Text = $"Desperdicado: {ByteFormatter.Format(g.WastedBytes)}", FontSize = 11, Foreground = new SolidColorBrush(Colors.Red) });
            panel.Children.Add(headerRow);

            foreach (var p in g.Paths)
            {
                var pathBox = new Border
                {
                    Background = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"],
                    CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 3, 6, 3), Margin = new Thickness(0, 2, 0, 2),
                };
                pathBox.Child = new TextBlock
                {
                    Text = p, FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis,
                    FontFamily = new FontFamily("Cascadia Code, Consolas, monospace"),
                };
                panel.Children.Add(pathBox);
            }

            card.Child = panel;
            _contentStack.Children.Add(card);
        }
    }
}