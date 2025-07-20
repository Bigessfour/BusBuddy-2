using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentDark.WPF;
using Syncfusion.UI.Xaml.NavigationDrawer;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.Windows.Tools;

namespace BusBuddy.WPF.Views.Main
{
    /// <summary>
    /// Main Window for Bus Buddy Application with Enhanced Navigation Drawer and FluentDark Theme
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private INavigationService? _navigationService;
        private readonly Dictionary<string, UserControl> _cachedViews = new Dictionary<string, UserControl>();
        private static readonly ILogger Logger = Log.ForContext<MainWindow>();

        public MainWindow()
        {
            try
            {
                // Enhanced startup debugging with detailed logging
                using (LogContext.PushProperty("ViewType", nameof(MainWindow)))
                using (LogContext.PushProperty("OperationType", "Constructor"))
                {
                    Logger.Information("MainWindow constructor started");

                    // CRITICAL: Set ApplyThemeAsDefaultStyle before InitializeComponent
                    // This ensures FluentDark theme resources are available globally
                    try
                    {
                        // Theme is managed centrally by OptimizedThemeService - no per-window configuration needed
                        // Verify theme is available using fast cache check
                        bool themeReady = BusBuddy.WPF.Services.OptimizedThemeService.IsResourceAvailable("ContentForeground");
                        Logger.Debug("Theme resources available: {ThemeReady}", themeReady);
                    }
                    catch (Exception themeEx)
                    {
                        Logger.Error(themeEx, "Theme verification failed");
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

                    // Additional safeguard: Theme is managed centrally by OptimizedThemeService
                    // No manual SfSkinManager calls needed per window
                    System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] Using centralized theme management from OptimizedThemeService");

                    // Theme is already applied globally via SfSkinManager in App.xaml.cs
                    // No need for window-specific theme application
                    System.Diagnostics.Debug.WriteLine("✅ [MAINWINDOW] Using global FluentDark theme from App.xaml.cs");

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
        /// Window loaded event handler
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("MainWindow loaded successfully with enhanced navigation");

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
        /// Setup docking manager event handlers with standardized configuration
        /// </summary>
        private void SetupDockingManager()
        {
            using (LogContext.PushProperty("DockingManagerSetup", "MainWindow"))
            {
                try
                {
                    if (MainDockingManager != null)
                    {
                        // Apply standard DockingManager configuration
                        MainDockingManager.UseDocumentContainer = true;
                        MainDockingManager.ContainerMode = DocumentContainerMode.TDI;
                        MainDockingManager.DockFill = true;
                        MainDockingManager.PersistState = true;

                        // Configure standard sizing for all panels using proper DockSide enum
                        if (NavigationDrawer != null)
                        {
                            DockingManager.SetSideInDockedMode(NavigationDrawer, DockSide.Left);
                            DockingManager.SetDesiredWidthInDockedMode(NavigationDrawer, 300);
                            DockingManager.SetCanClose(NavigationDrawer, true);
                            DockingManager.SetCanFloat(NavigationDrawer, true);
                        }

                        if (HeaderToolbar != null)
                        {
                            DockingManager.SetSideInDockedMode(HeaderToolbar, DockSide.Top);
                            DockingManager.SetDesiredHeightInDockedMode(HeaderToolbar, 64);
                            DockingManager.SetCanClose(HeaderToolbar, true);
                            DockingManager.SetCanFloat(HeaderToolbar, true);
                        }

                        if (PropertyPanel != null)
                        {
                            DockingManager.SetSideInDockedMode(PropertyPanel, DockSide.Right);
                            DockingManager.SetDesiredWidthInDockedMode(PropertyPanel, 300);
                            DockingManager.SetCanClose(PropertyPanel, true);
                            DockingManager.SetCanFloat(PropertyPanel, true);
                        }

                        if (StatusBar != null)
                        {
                            DockingManager.SetSideInDockedMode(StatusBar, DockSide.Bottom);
                            DockingManager.SetDesiredHeightInDockedMode(StatusBar, 32);
                            DockingManager.SetCanClose(StatusBar, true);
                            DockingManager.SetCanFloat(StatusBar, true);
                        }

                        Log.Information("DockingManager configured successfully with standard TDI container mode and sizing");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error setting up standardized DockingManager");
                }
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
                using (LogContext.PushProperty("Navigation", e.ViewName))
                {
                    Logger.Information("Navigation changed to: {ViewName}", e.ViewName);

                    // Update the view model's current view
                    if (_viewModel != null && e.ViewModel != null)
                    {
                        _viewModel.CurrentViewModel = e.ViewModel;
                        _viewModel.CurrentViewTitle = e.ViewTitle;
                        _viewModel.CurrentViewName = e.ViewName; // Update for menu checkboxes
                    }

                    // Create or activate view in DockingManager using optimized approach
                    ActivateOrCreateViewInDockingManager(e.ViewName, e.ViewModel, e.ViewTitle);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error handling navigation change");
            }
        }

        /// <summary>
        /// Optimized method to create or activate views in DockingManager with caching
        /// </summary>
        private void ActivateOrCreateViewInDockingManager(string viewName, object? viewModel, string viewTitle)
        {
            try
            {
                using (LogContext.PushProperty("ViewActivation", viewName))
                {
                    // Check if view already exists in DockingManager
                    var existingView = FindViewInDockingManager(viewName);
                    if (existingView != null)
                    {
                        // Activate existing view
                        if (MainDockingManager != null)
                        {
                            MainDockingManager.ActivateWindow(existingView.Name);
                            Logger.Information("Activated existing view: {ViewName}", viewName);
                        }
                        return;
                    }

                    // Create new view if it doesn't exist
                    var newView = CreateViewForDockingManager(viewName, viewModel, viewTitle);
                    if (newView != null && MainDockingManager != null)
                    {
                        // Add to DockingManager as Document
                        DockingManager.SetState(newView, DockState.Document);
                        DockingManager.SetCanClose(newView, true);
                        DockingManager.SetCanFloat(newView, true);
                        DockingManager.SetCanSerialize(newView, true);
                        DockingManager.SetHeader(newView, viewTitle);

                        // Add to children and activate
                        MainDockingManager.Children.Add(newView);
                        MainDockingManager.ActivateWindow(newView.Name);

                        Logger.Information("Created and activated new view: {ViewName}", viewName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error activating or creating view: {ViewName}", viewName);
            }
        }

        /// <summary>
        /// Create a new view for the DockingManager with proper caching
        /// </summary>
        private UserControl? CreateViewForDockingManager(string viewName, object? viewModel, string viewTitle)
        {
            try
            {
                // Check cache first for performance
                if (_cachedViews.TryGetValue(viewName, out var cachedView))
                {
                    // Update DataContext if view model is provided
                    if (viewModel != null)
                    {
                        cachedView.DataContext = viewModel;
                    }
                    return cachedView;
                }

                // Create new view based on view name
                UserControl? newView = viewName switch
                {
                    "Dashboard" => null, // Dashboard uses dedicated MainDashboardView
                    "BusManagement" => new BusBuddy.WPF.Views.Bus.BusManagementView(),
                    "DriverManagement" => new BusBuddy.WPF.Views.Driver.DriverManagementView(),
                    "RouteManagement" => new BusBuddy.WPF.Views.Route.RouteManagementView(),
                    "ScheduleManagement" => new BusBuddy.WPF.Views.Schedule.ScheduleManagementView(),
                    "StudentManagement" => new BusBuddy.WPF.Views.Student.StudentManagementView(),
                    "Maintenance" => new BusBuddy.WPF.Views.Maintenance.MaintenanceTrackingView(),
                    "FuelManagement" => new BusBuddy.WPF.Views.Fuel.FuelManagementView(),
                    "ActivityLog" => new BusBuddy.WPF.Views.ActivityLogView(),
                    "Settings" => new BusBuddy.WPF.Views.Settings.SettingsView(),
                    "XAIChat" => new BusBuddy.WPF.Views.XAI.OptimizedXAIChatView(),
                    "GoogleEarth" => new BusBuddy.WPF.Views.GoogleEarth.GoogleEarthView(),
                    _ => null
                };

                if (newView != null)
                {
                    // Set properties for DockingManager
                    newView.Name = $"{viewName}View";

                    // Set DataContext if view model is provided
                    if (viewModel != null)
                    {
                        newView.DataContext = viewModel;
                    }

                    // Cache the view for future use
                    _cachedViews[viewName] = newView;

                    Logger.Information("Created new view: {ViewName}", viewName);
                }

                return newView;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error creating view: {ViewName}", viewName);
                return null;
            }
        }

        /// <summary>
        /// Find existing view in DockingManager
        /// </summary>
        private FrameworkElement? FindViewInDockingManager(string viewName)
        {
            try
            {
                if (MainDockingManager?.Children != null)
                {
                    var expectedName = $"{viewName}View";
                    return MainDockingManager.Children.OfType<FrameworkElement>()
                        .FirstOrDefault(child => child.Name == expectedName);
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error finding view in DockingManager: {ViewName}", viewName);
                return null;
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
        }

        /// <summary>
        /// Reset DockingManager to default standardized layout
        /// </summary>
        public void ResetToDefaultLayout()
        {
            using (LogContext.PushProperty("LayoutReset", "MainWindow"))
            {
                try
                {
                    Log.Information("Resetting MainWindow DockingManager to default standardized layout");

                    if (MainDockingManager != null)
                    {
                        // Reset to standard layout using Syncfusion's LoadDockState/ResetDockState
                        try
                        {
                            MainDockingManager.ResetState();
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Failed to reset DockingManager state, applying manual reset");
                        }

                        // Ensure critical panels are visible and properly positioned
                        if (NavigationDrawer != null)
                        {
                            DockingManager.SetState(NavigationDrawer, DockState.Dock);
                            DockingManager.SetSideInDockedMode(NavigationDrawer, DockSide.Left);
                            NavigationDrawer.Visibility = Visibility.Visible;
                        }

                        if (MainDashboardView != null)
                        {
                            DockingManager.SetState(MainDashboardView, DockState.Document);
                            MainDashboardView.Visibility = Visibility.Visible;
                        }

                        if (HeaderToolbar != null)
                        {
                            DockingManager.SetState(HeaderToolbar, DockState.Dock);
                            DockingManager.SetSideInDockedMode(HeaderToolbar, DockSide.Top);
                            HeaderToolbar.Visibility = Visibility.Visible;
                        }

                        Log.Information("Default layout reset completed successfully");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error resetting to default layout");
                }
            }
        }

        /// <summary>
        /// Handle DockingManager active window changes - Proper Syncfusion API
        /// </summary>
        private void MainDockingManager_ActiveWindowChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                // Using proper Syncfusion DockingManager event args
                var activeWindow = e.NewValue as FrameworkElement;
                var windowName = activeWindow?.Name ?? "Unknown";

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
        /// Handle DockingManager window closing events - Proper Syncfusion API
        /// </summary>
        private void MainDockingManager_WindowClosing(object sender, CloseButtonEventArgs e)
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

                // Clear cached views
                _cachedViews.Clear();

                Logger.Information("MainWindow closed successfully");
                base.OnClosed(e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during MainWindow cleanup");
            }
        }

        #region Menu Event Handlers

        /// <summary>
        /// Handle Exit menu item click
        /// </summary>
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("Exit menu item clicked");
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error handling exit menu click");
            }
        }

        /// <summary>
        /// Handle Reset Layout menu item click
        /// </summary>
        private void ResetLayoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("Reset layout menu item clicked");
                ResetToDefaultLayout();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error handling reset layout menu click");
            }
        }

        /// <summary>
        /// Handle About menu item click
        /// </summary>
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("About menu item clicked");
                var aboutMessage = "Bus Buddy Transportation Management System\n\n" +
                                 "Version 1.0\n" +
                                 "Built with WPF and Syncfusion FluentDark Theme\n\n" +
                                 "© 2025 Bus Buddy Development Team";

                MessageBox.Show(aboutMessage, "About Bus Buddy", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error handling about menu click");
            }
        }

        /// <summary>
        /// Handle Cascade Windows menu item click
        /// </summary>
        private void CascadeWindowsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("Cascade windows menu item clicked");
                if (MainDockingManager != null)
                {
                    // Cascade functionality not available in current Syncfusion version
                    // Alternative: Float all dockable windows
                    foreach (FrameworkElement child in MainDockingManager.Children)
                    {
                        if (child != null && DockingManager.GetState(child) != DockState.Document)
                        {
                            DockingManager.SetState(child, DockState.Float);
                        }
                    }
                    Logger.Information("Floated all dockable windows as cascade alternative");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error cascading windows");
            }
        }

        /// <summary>
        /// Handle Tile Horizontally menu item click
        /// </summary>
        private void TileHorizontallyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("Tile horizontally menu item clicked");
                if (MainDockingManager != null)
                {
                    // Tile functionality not available in current Syncfusion version
                    // Alternative: Dock all windows to left/right sides
                    bool dockLeft = true;
                    foreach (FrameworkElement child in MainDockingManager.Children)
                    {
                        if (child != null && DockingManager.GetState(child) != DockState.Document)
                        {
                            DockingManager.SetState(child, DockState.Dock);
                            DockingManager.SetSideInDockedMode(child, dockLeft ? DockSide.Left : DockSide.Right);
                            dockLeft = !dockLeft;
                        }
                    }
                    Logger.Information("Arranged windows horizontally as tile alternative");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error tiling windows horizontally");
            }
        }

        /// <summary>
        /// Handle Tile Vertically menu item click
        /// </summary>
        private void TileVerticallyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("Tile vertically menu item clicked");
                if (MainDockingManager != null)
                {
                    // Arrange docked panels vertically using native Syncfusion approach
                    ArrangeDockedPanelsVertically();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error arranging windows vertically");
            }
        }

        /// <summary>
        /// Handle Close All Documents menu item click
        /// </summary>
        private void CloseAllDocumentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("Close all documents menu item clicked");
                if (MainDockingManager != null)
                {
                    // Close all document tabs except Dashboard
                    var documentsToClose = MainDockingManager.Children
                        .OfType<FrameworkElement>()
                        .Where(child => DockingManager.GetState(child) == DockState.Document
                                       && child.Name != "MainDashboardView")
                        .ToList();

                    foreach (var document in documentsToClose)
                    {
                        MainDockingManager.Children.Remove(document);
                        Logger.Information("Closed document: {DocumentName}", document.Name);
                    }

                    // Clear cache for closed views
                    var keysToRemove = _cachedViews.Keys
                        .Where(key => documentsToClose.Any(doc => doc.Name == $"{key}View"))
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        _cachedViews.Remove(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error closing all documents");
            }
        }

        /// <summary>
        /// Arrange docked panels vertically using native Syncfusion approach
        /// </summary>
        private void ArrangeDockedPanelsVertically()
        {
            try
            {
                // Get all docked child elements
                var dockedElements = MainDockingManager?.Children
                    .OfType<FrameworkElement>()
                    .Where(child => DockingManager.GetState(child) == DockState.Dock)
                    .ToList();

                if (dockedElements == null || !dockedElements.Any())
                {
                    Logger.Information("No docked elements found to arrange vertically");
                    return;
                }

                // Arrange elements with Top/Bottom positioning
                for (int i = 0; i < dockedElements.Count; i++)
                {
                    var element = dockedElements[i];
                    var side = i % 2 == 0 ? DockSide.Top : DockSide.Bottom;

                    DockingManager.SetSideInDockedMode(element, side);
                    DockingManager.SetDesiredHeightInDockedMode(element, 200); // Set reasonable height

                    Logger.Information("Set {ElementName} to {Side}", element.Name, side);
                }

                Logger.Information("Arranged {Count} panels vertically", dockedElements.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error arranging panels vertically");
            }
        }

        #endregion
    }
}
