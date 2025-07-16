using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Serilog;
using Serilog.Context;

namespace BusBuddy.WPF.Views.Activity
{
    public partial class ActivityLoggingView : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<ActivityLoggingView>();

        public ActivityLoggingView()
        {
            try
            {
                using (LogContext.PushProperty("ViewType", nameof(ActivityLoggingView)))
                using (LogContext.PushProperty("OperationType", "ViewInitialization"))
                {
                    Logger.Information("ActivityLoggingView initialization started");

                    InitializeComponent();

                    if (Application.Current is App app && app.Services != null)
                    {
                        var vm = app.Services.GetService<ViewModels.ActivityLoggingViewModel>();
                        if (vm != null)
                        {
                            DataContext = vm;
                            Logger.Information("ActivityLoggingViewModel successfully set as DataContext");
                        }
                        else
                        {
                            Logger.Warning("ActivityLoggingViewModel could not be resolved from DI container");
                        }
                    }

                    Logger.Information("ActivityLoggingView initialization completed successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize ActivityLoggingView: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
