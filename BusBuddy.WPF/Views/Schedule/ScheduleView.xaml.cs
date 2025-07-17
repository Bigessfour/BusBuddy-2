using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using BusBuddy.WPF.ViewModels.ScheduleManagement;
using BusBuddy.WPF;
using Serilog;
using Serilog.Context;

namespace BusBuddy.WPF.Views.Schedule
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// Sports Trips & Routes Schedule Management View
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        private static readonly ILogger _logger = Log.ForContext<ScheduleView>();

        public ScheduleView()
        {
            using (LogContext.PushProperty("LogCategory", "Schedules Log"))
            {
                _logger.Information("Initializing ScheduleView for Sports Trips & Routes management");

                try
                {
                    InitializeComponent();
                    InitializeViewModel();

                    _logger.Information("ScheduleView initialized successfully with sports scheduling capabilities");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to initialize ScheduleView");
                    throw;
                }
            }
        }

        private void InitializeViewModel()
        {
            using (LogContext.PushProperty("LogCategory", "Schedules Log"))
            {
                try
                {
                    if (System.Windows.Application.Current is App app && app.Services != null)
                    {
                        var viewModel = app.Services.GetRequiredService<ScheduleViewModel>();
                        DataContext = viewModel;

                        _logger.Information("ScheduleViewModel injected successfully with sports category filtering");
                    }
                    else
                    {
                        _logger.Warning("Could not resolve ScheduleViewModel from dependency injection container");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to initialize ScheduleViewModel for sports scheduling");
                    throw;
                }
            }
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            using (LogContext.PushProperty("LogCategory", "Schedules Log"))
            {
                _logger.Information("ScheduleView loaded — Sports Trips & Routes management view ready");
            }
        }

        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            using (LogContext.PushProperty("LogCategory", "Schedules Log"))
            {
                _logger.Information("ScheduleView unloaded — Disposing sports scheduling resources");
            }
        }
    }
}
