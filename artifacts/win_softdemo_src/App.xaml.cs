using System;
using System.IO;
using Microsoft.UI.Xaml;

namespace MotionSoftDemo;

/// <summary>
/// 派题 #8 (review item 4): Application.UnhandledException is subscribed exactly once, here.
/// Pages observe it through the static <see cref="Unhandled"/> event and must unsubscribe in Shutdown().
/// </summary>
public partial class App : Application
{
    private Window? _window;
    public static int UnhandledCount { get; private set; }
    public static event Action<Exception>? Unhandled;
    public static int UnhandledSubscribers => Unhandled?.GetInvocationList().Length ?? 0;

    public App()
    {
        InitializeComponent();
        UnhandledException += (s, ev) =>
        {
            UnhandledCount++;
            try { File.AppendAllText(Path.Combine(Path.GetTempPath(), "MotionSoftDemo_crash.txt"), DateTime.Now + " " + ev.Exception + Environment.NewLine); } catch { }
            try { Unhandled?.Invoke(ev.Exception); } catch { }
        };
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
