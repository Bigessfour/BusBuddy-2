using System.Windows.Controls;
using Serilog;

namespace BusBuddy.WPF.Controls
{
    /// <summary>
    /// Interaction logic for AddressValidationControl.xaml
    /// This is the XAML code-behind partial class.
    /// The main implementation is in AddressValidationControl.cs
    /// </summary>
    public partial class AddressValidationControl : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<AddressValidationControl>();

        // Event handlers and XAML-specific logic only
        // Main implementation is in AddressValidationControl.cs

        /// <summary>
        /// Event handler for the Validate Address button click
        /// </summary>
        private async void ValidateAddress_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Logger.Information("Validate Address button clicked");

                // Get the address from the text box (find control by name)
                var addressTextBox = this.FindName("AddressTextBox") as TextBox;
                var resultsTextBlock = this.FindName("ResultsTextBlock") as TextBlock;

                var address = addressTextBox?.Text?.Trim();

                if (string.IsNullOrEmpty(address))
                {
                    if (resultsTextBlock != null)
                        resultsTextBlock.Text = "Please enter an address to validate.";
                    return;
                }

                // Update the address properties from the text box
                Street = address;

                // Call the validation method from the main implementation
                await ValidateAddressAsync();
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex, "Error during address validation button click");
                var resultsTextBlock = this.FindName("ResultsTextBlock") as TextBlock;
                if (resultsTextBlock != null)
                    resultsTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }
}
