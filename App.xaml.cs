using Microsoft.UI.Xaml;

namespace DiskIntelligence;

public partial class App : Application
{
    public static Window? MainWindow { get; set; }
    private Window? _mainWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }
}