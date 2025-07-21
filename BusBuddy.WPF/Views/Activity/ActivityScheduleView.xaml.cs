using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.Utilities;
using Serilog;
using Syncfusion.SfSkinManager;

namespace BusBuddy.WPF.Views.Activity
{
    public partial class ActivityScheduleView : UserControl
    {
        private readonly DialogService _dialogService = new DialogService();
        private static readonly ILogger Logger = Log.ForContext<ActivityScheduleView>();

        public ActivityScheduleView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Apply FluentDark theme with FluentLight fallback
                Logger.Debug("🎨 Applying FluentDark theme to ActivityScheduleView");
                using var fluentDarkTheme = new Theme("FluentDark");
                SfSkinManager.SetTheme(this, fluentDarkTheme);
                Logger.Information("✅ FluentDark theme successfully applied to ActivityScheduleView");
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "⚠️ Failed to apply FluentDark theme, attempting FluentLight fallback");
                try
                {
                    using var fluentLightTheme = new Theme("FluentLight");
                    SfSkinManager.SetTheme(this, fluentLightTheme);
                    Logger.Information("✅ FluentLight fallback theme applied to ActivityScheduleView");
                }
                catch (Exception fallbackEx)
                {
                    Logger.Error(fallbackEx, "❌ Failed to apply both FluentDark and FluentLight themes to ActivityScheduleView");
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.Activity.ActivityScheduleViewModel;
            if (vm != null)
            {
                vm.OpenScheduleDialog(false);
                var result = _dialogService.ShowActivityScheduleDialog(vm);
                if (result == true)
                {
                    _ = vm.LoadSchedulesAsync();
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.Activity.ActivityScheduleViewModel;
            if (vm != null)
            {
                vm.OpenScheduleDialog(true);
                var result = _dialogService.ShowActivityScheduleDialog(vm);
                if (result == true)
                {
                    _ = vm.LoadSchedulesAsync();
                }
            }
        }
    }
}
