using System;
using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DeprecatedBannerControl.xaml
    /// </summary>
    public partial class DeprecatedBannerControl : UserControl
    {
        public static readonly DependencyProperty MainMessageProperty = DependencyProperty.Register(
            nameof(MainMessage), typeof(string), typeof(DeprecatedBannerControl),
            new PropertyMetadata("This module has been deprecated and will be removed in a future version.", OnMainMessageChanged));

        public static readonly DependencyProperty DetailMessageProperty = DependencyProperty.Register(
            nameof(DetailMessage), typeof(string), typeof(DeprecatedBannerControl),
            new PropertyMetadata("Please use the alternative module for this functionality.", OnDetailMessageChanged));

        public static readonly DependencyProperty LearnMoreUriProperty = DependencyProperty.Register(
            nameof(LearnMoreUri), typeof(string), typeof(DeprecatedBannerControl),
            new PropertyMetadata(string.Empty, OnLearnMoreUriChanged));

        public string MainMessage
        {
            get => (string)GetValue(MainMessageProperty);
            set => SetValue(MainMessageProperty, value);
        }

        public string DetailMessage
        {
            get => (string)GetValue(DetailMessageProperty);
            set => SetValue(DetailMessageProperty, value);
        }

        public string LearnMoreUri
        {
            get => (string)GetValue(LearnMoreUriProperty);
            set => SetValue(LearnMoreUriProperty, value);
        }

        public DeprecatedBannerControl()
        {
            InitializeComponent();
        }

        private static void OnMainMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeprecatedBannerControl control && e.NewValue is string message)
            {
                control.MainMessageText.Text = message;
            }
        }

        private static void OnDetailMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeprecatedBannerControl control && e.NewValue is string message)
            {
                control.DetailMessageText.Text = message;
            }
        }

        private static void OnLearnMoreUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeprecatedBannerControl control)
            {
                control.LearnMoreButton.Visibility = string.IsNullOrEmpty((string)e.NewValue)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private void LearnMoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LearnMoreUri))
            {
                try
                {
                    // If it's a valid URI, open in browser
                    if (Uri.TryCreate(LearnMoreUri, UriKind.Absolute, out var uri))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = uri.AbsoluteUri,
                            UseShellExecute = true
                        });
                    }
                    // Otherwise, it might be an internal page or help topic
                    else
                    {
                        // Raise an event that the containing view can handle for navigation
                        LearnMoreRequested?.Invoke(this, new LearnMoreRequestedEventArgs(LearnMoreUri));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open help documentation: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event EventHandler<LearnMoreRequestedEventArgs>? LearnMoreRequested;
    }

    public class LearnMoreRequestedEventArgs : EventArgs
    {
        public string ResourceIdentifier { get; }

        public LearnMoreRequestedEventArgs(string resourceIdentifier)
        {
            ResourceIdentifier = resourceIdentifier;
        }
    }
}
