using BusBuddy.WPF.ViewModels;
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

        // Configure Syncfusion theme before InitializeComponent
        SfSkinManager.ApplyThemeAsDefaultStyle = true;
        SfSkinManager.SetTheme(this, new Theme("Office2019Colorful"));

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
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _logger?.LogInformation("MainWindow closed");
    }
}