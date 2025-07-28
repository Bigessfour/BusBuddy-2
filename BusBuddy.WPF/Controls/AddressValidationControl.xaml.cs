using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF.Controls
{
    /// <summary>
    /// Interaction logic for AddressValidationControl.xaml
    /// This is the XAML code-behind partial class.
    /// The main implementation is in AddressValidationControl.cs
    /// </summary>
    public partial class AddressValidationControl
    {
        // The ValidateAddress_Click event handler that connects to the XAML button
        private async void ValidateAddress_Click(object sender, RoutedEventArgs e)
        {
            await ValidateAddressAsync();
        }
    }
}
