using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.Core.Services;
using BusBuddy.WPF.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.Student
{
    public partial class StudentEditDialog
    {
        // This is a partial class implementation that will be complemented by
        // the code generated from the XAML.

        // The AddressValidator control declared in XAML
        private AddressValidationControl? _addressValidator;

        // Called when the dialog is loaded
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Find the AddressValidator control defined in XAML
            _addressValidator = FindName("AddressValidator") as AddressValidationControl;

            // Get the address validation service from DI
            var addressValidationService = ((App)Application.Current).Services.GetService<IAddressValidationService>();
            if (addressValidationService != null && _addressValidator != null)
            {
                // Set the service in our control
                _addressValidator.DataContext = new Controls.AddressValidationControl(addressValidationService);
            }
        }
    }
}
