using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Windows;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow>? _logger;

    public MainWindow()
    {
        var stopwatch = Stopwatch.StartNew();

        InitializeComponent();

        this.WindowState = WindowState.Maximized;
        this.WindowStyle = WindowStyle.SingleBorderWindow;
        this.ResizeMode = ResizeMode.CanResize;

        stopwatch.Stop();

        // Try to get logger from application services
        _logger = (Application.Current as App)?.Services?.GetService(typeof(ILogger<MainWindow>)) as ILogger<MainWindow>;
        _logger?.LogInformation("[WINDOW_PERF] MainWindow initialization completed in {DurationMs}ms", stopwatch.ElapsedMilliseconds);
    }
}