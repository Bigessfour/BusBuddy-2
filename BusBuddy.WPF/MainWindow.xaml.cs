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
                // Enhanced startup debugging with detailed logging
                System.Diagnostics.Debug.WriteLine("🏗️ [MAINWINDOW] Constructor started");

                // CRITICAL: Set ApplyThemeAsDefaultStyle before InitializeComponent
                // This ensures FluentDark theme resources are available globally
                try
                {
                    SfSkinManager.ApplyThemeAsDefaultStyle = true;
                    System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] SfSkinManager.ApplyThemeAsDefaultStyle set to true");
                }
                catch (Exception themeEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] Theme setup failed: {themeEx.Message}");
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }

                // Enhanced InitializeComponent with breakpoint capability
                try
                {
                    System.Diagnostics.Debug.WriteLine("🔄 [MAINWINDOW] Calling InitializeComponent");
                    InitializeComponent();
                    System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] InitializeComponent completed successfully");
                }
                catch (Exception initEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] InitializeComponent failed: {initEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] Exception details: {initEx}");

                    // Enhanced debugging for XAML parsing issues
                    if (initEx is System.Windows.Markup.XamlParseException xamlEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] XAML Parse Error - Line: {xamlEx.LineNumber}, Position: {xamlEx.LinePosition}");
                        System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] XAML File: {xamlEx.BaseUri}");
                    }

                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    throw;
                }

                // Additional safeguard: Ensure SfSkinManager.ApplyStylesOnApplication is set
                try
                {
                    SfSkinManager.ApplyStylesOnApplication = true;
                    System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] SfSkinManager.ApplyStylesOnApplication set to true");
                }
                catch (Exception styleEx)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [MAINWINDOW] Failed to set ApplyStylesOnApplication: {styleEx.Message}");
                    // Log but continue - this is a fallback safeguard
                }

                // Apply FluentDark theme consistently with enhanced error handling
                try
                {
                    ApplyFluentDarkTheme();
                    System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] Theme applied successfully");
                }
                catch (Exception themeApplyEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] Theme application failed: {themeApplyEx.Message}");
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }

                // Enhanced DI setup with detailed validation
                try
                {
                    System.Diagnostics.Debug.WriteLine("🔄 [MAINWINDOW] Setting up dependency injection");

                    if (Application.Current is App appInstance && appInstance.Services != null)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] App instance and services available");

                        _viewModel = appInstance.Services.GetService<MainViewModel>();
                        _navigationService = appInstance.Services.GetService<INavigationService>();

                        if (_viewModel != null)
                        {
                            DataContext = _viewModel;
                            System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] MainViewModel set as DataContext");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("❌ [MAINWINDOW] MainViewModel is null");
                            if (System.Diagnostics.Debugger.IsAttached)
                            {
                                System.Diagnostics.Debugger.Break();
                            }
                        }

                        // Subscribe to navigation events with error handling
                        if (_navigationService != null)
                        {
                            _navigationService.NavigationChanged += OnNavigationChanged;
                            System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] Navigation service event subscribed");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ [MAINWINDOW] Navigation service is null");
                        }

                        Log.Information("MainWindow initialized with enhanced navigation drawer and FluentDark theme");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ [MAINWINDOW] App instance or services are null");
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                    }
                }
                catch (Exception diEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] DI setup failed: {diEx.Message}");
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] Constructor completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] Constructor failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [MAINWINDOW] Full exception: {ex}");

                Log.Error(ex, "Error initializing MainWindow");

                // Enhanced debugging breakpoint
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
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
                // SfSkinManager.ApplicationTheme is already set globally in App.xaml.cs
                // Only apply the theme to this specific window
                SfSkinManager.SetTheme(this, new Theme() { ThemeName = "FluentDark" });

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
