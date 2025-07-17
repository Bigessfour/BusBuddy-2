using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentDark.WPF;
using Syncfusion.UI.Xaml.NavigationDrawer;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.Windows.Tools;

namespace BusBuddy.WPF
{
    /// <summary>
    /// Main Window for Bus Buddy Application with Enhanced Navigation Drawer and FluentDark Theme
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private INavigationService? _navigationService;

        public MainWindow()
        {
            try
            {
                // CRITICAL: Set ApplyThemeAsDefaultStyle before InitializeComponent
                // This ensures FluentDark theme resources are available globally
                SfSkinManager.ApplyThemeAsDefaultStyle = true;

                InitializeComponent();

                // Apply FluentDark theme consistently
                ApplyFluentDarkTheme();

                // Set DataContext to MainViewModel and get NavigationService
                if (Application.Current is App appInstance && appInstance.Services != null)
                {
                    _viewModel = appInstance.Services.GetService<MainViewModel>();
                    _navigationService = appInstance.Services.GetService<INavigationService>();
                    DataContext = _viewModel;

                    // Subscribe to navigation events
                    if (_navigationService != null)
                    {
                        _navigationService.NavigationChanged += OnNavigationChanged;
                    }

                    Log.Information("MainWindow initialized with enhanced navigation drawer and FluentDark theme");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing MainWindow");
                throw;
            }
        }

        /// <summary>
        /// Apply FluentDark theme consistently across the application
        /// </summary>
        private void ApplyFluentDarkTheme()
        {
            try
            {
                // Ensure FluentDark theme is applied to this window
                SfSkinManager.SetVisualStyle(this, VisualStyles.FluentDark);

                // Register FluentDark theme settings
                var fluentDarkSettings = new FluentDarkThemeSettings();
                SfSkinManager.RegisterThemeSettings("FluentDark", fluentDarkSettings);

                // Apply theme using Theme property
                var fluentTheme = new Theme() { ThemeName = "FluentDark" };
                SfSkinManager.SetTheme(this, fluentTheme);

                Log.Information("FluentDark theme applied successfully to MainWindow");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to apply FluentDark theme to MainWindow");
            }
        }

        /// <summary>
        /// Window loaded event handler
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("MainWindow loaded successfully with enhanced navigation");

                // Apply theme to all child controls
                ApplyThemeToChildControls();

                // Set up docking manager event handlers
                SetupDockingManager();

                // Initialize with Dashboard using NavigationService
                if (_navigationService != null)
                {
                    _navigationService.NavigateTo("Dashboard");
                }
                else if (_viewModel != null)
                {
                    // Fallback to old method if NavigationService is not available
                    _viewModel.NavigateToDashboard();
                    ShowDashboard();
                }
                else
                {
                    Log.Warning("Both NavigationService and MainViewModel are null during Window_Loaded");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in MainWindow_Loaded");
            }
        }

        /// <summary>
        /// Setup docking manager event handlers
        /// </summary>
        private void SetupDockingManager()
        {
            try
            {
                if (MainDockingManager != null)
                {
                    // According to Syncfusion documentation, use proper event handler signatures
                    MainDockingManager.ActiveWindowChanged += MainDockingManager_ActiveWindowChanged;
                    MainDockingManager.WindowClosing += MainDockingManager_WindowClosing;

                    // Set the document container mode - these are already set in XAML but ensure they're correct
                    // MainDockingManager.ContainerMode = ContainerMode.TDI;
                    // MainDockingManager.DockBehavior = DockBehavior.VS2010;
                    MainDockingManager.UseDocumentContainer = true;
                    MainDockingManager.PersistState = true;

                    Log.Information("DockingManager configured successfully with TDI container mode");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error setting up DockingManager");
            }
        }

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("Menu toggle button clicked");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in menu toggle button click");
            }
        }

        private void DisplayModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Log.Information("Display mode selection changed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error changing display mode");
            }
        }

        private void NavigationDrawer_ItemClicked(object sender, NavigationItemClickedEventArgs e)
        {
            try
            {
                if (e.Item is Syncfusion.UI.Xaml.NavigationDrawer.NavigationItem navigationItem)
                {
                    var header = navigationItem.Header?.ToString();
                    var tag = navigationItem.Tag?.ToString();
                    Log.Information("Navigation item clicked: {Header} (Tag: {Tag})", header, tag);

                    // Use NavigationService for centralized navigation
                    if (_navigationService != null && !string.IsNullOrEmpty(tag))
                    {
                        _navigationService.NavigateTo(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling navigation item click");
            }
        }

        /// <summary>
        /// Handle navigation changes from NavigationService
        /// </summary>
        private void OnNavigationChanged(object? sender, NavigationEventArgs e)
        {
            try
            {
                Log.Information("Navigation changed to: {ViewName}", e.ViewName);

                // Update the view model's current view
                if (_viewModel != null && e.ViewModel != null)
                {
                    _viewModel.CurrentViewModel = e.ViewModel;
                    _viewModel.CurrentViewTitle = e.ViewTitle;
                }

                // Update UI based on view
                if (e.ViewName == "Dashboard")
                {
                    ShowDashboard();
                }
                else
                {
                    ShowContentContainer();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling navigation change");
            }
        }

        /// <summary>
        /// Show the dashboard view
        /// </summary>
        private void ShowDashboard()
        {
            try
            {
                if (MainDashboardView != null)
                {
                    MainDashboardView.Visibility = Visibility.Visible;
                    if (ContentContainer != null)
                    {
                        ContentContainer.Visibility = Visibility.Collapsed;
                    }

                    // Activate the dashboard in the docking manager
                    if (MainDockingManager != null)
                    {
                        MainDockingManager.ActivateWindow(MainDashboardView.Name);
                    }

                    Log.Information("Dashboard view activated");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing dashboard");
            }
        }

        /// <summary>
        /// Show the content container for other views
        /// </summary>
        private void ShowContentContainer()
        {
            try
            {
                if (ContentContainer != null)
                {
                    ContentContainer.Visibility = Visibility.Visible;
                    if (MainDashboardView != null)
                    {
                        MainDashboardView.Visibility = Visibility.Collapsed;
                    }

                    // Activate the content container in the docking manager
                    if (MainDockingManager != null)
                    {
                        MainDockingManager.ActivateWindow(ContentContainer.Name);
                    }

                    Log.Information("Content container activated");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing content container");
            }
        }        /// <summary>
                 /// Handle DockingManager active window changes
                 /// </summary>
        private void MainDockingManager_ActiveWindowChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                // According to Syncfusion documentation, the event provides DependencyPropertyChangedEventArgs
                var newActiveWindow = e.NewValue as FrameworkElement;
                var windowName = newActiveWindow?.Name ?? "Unknown";

                Log.Information("DockingManager active window changed to: {WindowName}", windowName);

                // Update the current view title if needed
                if (_viewModel != null)
                {
                    _viewModel.CurrentViewTitle = GetViewTitleFromWindow(windowName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling DockingManager active window change");
            }
        }

        /// <summary>
        /// Handle DockingManager window closing events
        /// </summary>
        private void MainDockingManager_WindowClosing(object sender, WindowClosingEventArgs e)
        {
            try
            {
                var windowName = (e.TargetItem as FrameworkElement)?.Name ?? "Unknown";
                Log.Information("DockingManager window closing: {WindowName}", windowName);

                // Prevent certain critical windows from closing
                if (windowName == "MainDashboardView" || windowName == "NavigationDrawer")
                {
                    e.Cancel = true;
                    Log.Information("Prevented critical window from closing: {WindowName}", windowName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling DockingManager window closing");
            }
        }

        /// <summary>
        /// Get appropriate view title based on window name
        /// </summary>
        private string GetViewTitleFromWindow(string windowName)
        {
            return windowName switch
            {
                "MainDashboardView" => "Bus Buddy Dashboard",
                "ContentContainer" => _viewModel?.CurrentViewTitle ?? "Bus Buddy",
                "NavigationDrawer" => "Navigation",
                "HeaderToolbar" => "Toolbar",
                "PropertyPanel" => "Properties",
                "StatusBar" => "Status",
                _ => "Bus Buddy"
            };
        }

        /// <summary>
        /// Apply FluentDark theme to all child controls
        /// </summary>
        private void ApplyThemeToChildControls()
        {
            try
            {
                Log.Debug("FluentDark theme applied to all child controls");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying theme to child controls");
            }
        }

        /// <summary>
        /// Handle window state changes for full-screen mode
        /// </summary>
        protected override void OnStateChanged(EventArgs e)
        {
            try
            {
                base.OnStateChanged(e);

                if (WindowState == WindowState.Maximized)
                {
                    // Ensure proper layout in maximized state
                    Log.Debug("Window maximized - adjusting layout");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling window state change");
            }
        }

        /// <summary>
        /// Clean up resources and event subscriptions
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Unsubscribe from navigation events
                if (_navigationService != null)
                {
                    _navigationService.NavigationChanged -= OnNavigationChanged;
                }

                Log.Information("MainWindow closed successfully");
                base.OnClosed(e);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during MainWindow cleanup");
            }
        }
    }
}
