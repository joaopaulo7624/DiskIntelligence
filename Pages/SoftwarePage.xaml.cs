using DiskIntelligence.Helpers;
using DiskIntelligence.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DiskIntelligence.Pages;

public sealed partial class SoftwarePage : Page
{
    public MainViewModel ViewModel { get; }
    private StackPanel _contentStack = null!;

    public SoftwarePage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        _contentStack = new StackPanel { Padding = new Thickness(24), Spacing = 16 };
        RootGrid.Children.Add(new ScrollViewer { Content = _contentStack });
        ViewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ViewModel.InstalledSoftware)) Refresh(); };
        BuildUI();
    }

    private void BuildUI()
    {
        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var title = new TextBlock { Text = "Programas Instalados", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] };
        Grid.SetColumn(title, 0);
        header.Children.Add(title);

        var refreshBtn = new Button { Content = "Atualizar" };
        refreshBtn.Click += (_, _) => ViewModel.LoadSoftwareCommand.Execute(null);
        Grid.SetColumn(refreshBtn, 1);
        header.Children.Add(refreshBtn);

        _contentStack.Children.Add(header);
        Refresh();
        ViewModel.LoadSoftwareCommand.Execute(null);
    }

    private void Refresh()
    {
        _contentStack.Children.Clear();
        BuildUI_Header();

        var list = new ListView { SelectionMode = ListViewSelectionMode.None };
        list.ItemsSource = ViewModel.InstalledSoftware;
        _contentStack.Children.Add(list);
    }

    private void BuildUI_Header()
    {
        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var title = new TextBlock { Text = $"Programas Instalados ({ViewModel.InstalledSoftware.Count})", Style = (Style)Application.Current.Resources["TitleTextBlockStyle"] };
        Grid.SetColumn(title, 0);
        header.Children.Add(title);

        var refreshBtn = new Button { Content = "Atualizar" };
        refreshBtn.Click += (_, _) => ViewModel.LoadSoftwareCommand.Execute(null);
        Grid.SetColumn(refreshBtn, 1);
        header.Children.Add(refreshBtn);

        _contentStack.Children.Add(header);
    }
}