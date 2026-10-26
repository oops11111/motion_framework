using Microsoft.UI.Xaml;

namespace MotionSoftDemo;

/// <summary>
/// The application window. 派题 #8 (review item 2): WinUI 3 does not guarantee Page.Unloaded on window
/// close, so Closed also calls the page's idempotent Shutdown() (Rendering unsubscribe + fm_destroy).
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        RootFrame.Navigate(typeof(MainPage));
        Closed += (_, __) => (RootFrame.Content as MainPage)?.Shutdown("MainWindow.Closed");
    }
}
