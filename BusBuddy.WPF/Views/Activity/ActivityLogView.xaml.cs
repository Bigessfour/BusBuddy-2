using BusBuddy.Core.Services;
using BusBuddy.WPF.ViewModels;
using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class ActivityLogView : UserControl
    {
        public ActivityLogView()
        {
            InitializeComponent();
            // For demo: resolve service from App DI
            var app = System.Windows.Application.Current as App;
            if (app?.Services != null)
            {
                var service = app.Services.GetService(typeof(IActivityLogService)) as IActivityLogService;
                DataContext = new ActivityLogViewModel(service!);
            }
        }
    }
}
