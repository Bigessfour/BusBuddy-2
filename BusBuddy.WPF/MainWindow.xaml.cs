using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.Logging;
using Microsoft.Extensions.Logging;
using Syncfusion.SfSkinManager;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow>? _logger;
    private bool _isNavigationPanelVisible = true;

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

        // Initialize navigation panel visibility
        UpdateNavigationPanelVisibility();

        // Subscribe to DataContext changes for proper ViewModel binding
        this.DataContextChanged += MainWindow_DataContextChanged;

        // CRITICAL FIX: Add proper window closing behavior
        this.Closing += MainWindow_Closing;
        this.Closed += MainWindow_Closed;
        this.Loaded += MainWindow_Loaded;
    }

    /// <summary>
    /// Toggle the navigation panel visibility
    /// </summary>
    private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isNavigationPanelVisible = !_isNavigationPanelVisible;
        UpdateNavigationPanelVisibility();

        _logger?.LogDebug("Navigation panel toggled. Visible: {IsVisible}", _isNavigationPanelVisible);
    }

    /// <summary>
    /// Update the navigation panel visibility
    /// </summary>
    private void UpdateNavigationPanelVisibility()
    {
        if (NavigationPanel != null)
        {
            NavigationPanel.Visibility = _isNavigationPanelVisible ? Visibility.Visible : Visibility.Collapsed;

            // Update grid column definitions for responsive layout
            var grid = NavigationPanel.Parent as Grid;
            if (grid?.ColumnDefinitions.Count >= 2)
            {
                grid.ColumnDefinitions[0].Width = _isNavigationPanelVisible
                    ? new GridLength(240)
                    : new GridLength(0);
            }
        }
    }

    /// <summary>
    /// Handle DataContext changes and ensure proper ViewModel binding
    /// </summary>
    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is MainViewModel mainViewModel)
        {
            _logger?.LogDebug("MainWindow DataContext set to MainViewModel");

            // Subscribe to CurrentViewModel changes to handle special cases
            mainViewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.CurrentViewModel))
                {
                    HandleCurrentViewModelChanged(mainViewModel.CurrentViewModel);
                }
            };

            // Handle initial current view model if already set
            if (mainViewModel.CurrentViewModel != null)
            {
                HandleCurrentViewModelChanged(mainViewModel.CurrentViewModel);
            }
        }
    }

    /// <summary>
    /// Handle special ViewModel binding requirements
    /// </summary>
    private void HandleCurrentViewModelChanged(object? currentViewModel)
    {
        _logger?.LogDebug("Current ViewModel changed to: {ViewModelType}",
            currentViewModel?.GetType().Name ?? "null");

        // For DashboardViewModel, ensure proper DataContext binding
        if (currentViewModel is DashboardViewModel dashboardViewModel)
        {
            _logger?.LogDebug("Setting up DashboardViewModel specific bindings");

            // The ContentControl with stretch alignment will automatically bind via DataTemplate
            // EnhancedDashboardView will inherit the DashboardViewModel as its DataContext

            try
            {
                // Ensure the main content control has the correct alignment
                if (MainContentControl != null)
                {
                    MainContentControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    MainContentControl.VerticalAlignment = VerticalAlignment.Stretch;
                    MainContentControl.Margin = new Thickness(0);

                    _logger?.LogDebug("MainContentControl configured with stretch alignment for DashboardViewModel");
                }

                _logger?.LogDebug("DashboardViewModel is ready for display with stretch alignment");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error setting up DashboardViewModel");
            }
        }
    }

    /// <summary>
    /// Handle window closing to cleanup resources
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        _logger?.LogInformation("MainWindow closing requested");

        // Allow the window to close normally
        // If you need confirmation, you can add it here:
        // var result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButton.YesNo);
        // if (result == MessageBoxResult.No)
        // {
        //     e.Cancel = true;
        //     return;
        // }

        base.OnClosing(e);
    }

    /// <summary>
    /// Handle window closed to cleanup resources
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _logger?.LogInformation("MainWindow closed - shutting down application");

        // Ensure application shuts down when main window closes
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Handle window loaded event for theme application
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply initial theme via SfSkinManager (not hardcoded in XAML)
        try
        {
            var app = (App)Application.Current;
            var themeService = app.Services?.GetService(typeof(BusBuddy.WPF.Services.IThemeService)) as BusBuddy.WPF.Services.IThemeService;
            if (themeService != null)
            {
                // Apply the current theme to this window
                SfSkinManager.SetTheme(this, new Theme(themeService.CurrentTheme));
                _logger?.LogInformation("[THEME] Applied theme {Theme} to MainWindow on load", themeService.CurrentTheme);
            }
            else
            {
                // Fallback to default theme
                SfSkinManager.SetTheme(this, new Theme("Office2019Colorful"));
                _logger?.LogWarning("[THEME] ThemeService not found, using default theme");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[THEME] Error applying theme on window load: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Handle window closing event
    /// </summary>
    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        _logger?.LogInformation("[WINDOW] MainWindow closing initiated by user (X button)");
        // Let the existing OnClosing method handle the logic
    }

    /// <summary>
    /// Handle window closed event
    /// </summary>
    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _logger?.LogInformation("[WINDOW] MainWindow closed event fired");
        // Let the existing OnClosed method handle the logic
    }
}