using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views.Ticket
{
    public partial class TicketManagementView : UserControl
    {
        public TicketManagementView()
        {
            try
            {
                InitializeComponent();                // Get the view model from DI
                if (System.Windows.Application.Current is App app && app.Services != null)
                {
                    var viewModel = app.Services.GetRequiredService<TicketManagementViewModel>();
                    DataContext = viewModel;

                    // Load data asynchronously using the RefreshCommand
                    viewModel.RefreshCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Ticket Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
