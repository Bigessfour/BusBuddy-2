using BusBuddy.WPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            if (Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetRequiredService<DashboardViewModel>();
            }
        }
    }
}
