using System;
using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF.Controls
{
    /// <summary>
    /// A visual control that displays a banner indicating a module is deprecated.
    /// </summary>
    public class DeprecatedBanner : Border
    {
        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(DeprecatedBanner),
            new PropertyMetadata("DEPRECATED MODULE"));

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(DeprecatedBanner),
            new PropertyMetadata("This module has been deprecated and will be removed in a future version."));

        public static readonly DependencyProperty AlternativeProperty = DependencyProperty.Register(
            nameof(Alternative), typeof(string), typeof(DeprecatedBanner),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RemovalDateProperty = DependencyProperty.Register(
            nameof(RemovalDate), typeof(DateTime?), typeof(DeprecatedBanner),
            new PropertyMetadata(null, OnRemovalDateChanged));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the title for the deprecated banner.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the message explaining why the module is deprecated.
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>
        /// Gets or sets information about alternative modules or features.
        /// </summary>
        public string Alternative
        {
            get => (string)GetValue(AlternativeProperty);
            set => SetValue(AlternativeProperty, value);
        }

        /// <summary>
        /// Gets or sets the planned removal date.
        /// </summary>
        public DateTime? RemovalDate
        {
            get => (DateTime?)GetValue(RemovalDateProperty);
            set => SetValue(RemovalDateProperty, value);
        }

        /// <summary>
        /// Gets the formatted removal date text if a date is specified.
        /// </summary>
        public string RemovalDateText => RemovalDate.HasValue
            ? $"Will be removed on {RemovalDate.Value:MMMM d, yyyy}"
            : string.Empty;

        #endregion

        private readonly TextBlock _titleBlock;
        private readonly TextBlock _messageBlock;
        private readonly TextBlock _alternativeBlock;
        private readonly TextBlock _removalDateBlock;

        public DeprecatedBanner()
        {
            // Create a stack panel to hold the text blocks
            var panel = new StackPanel
            {
                Margin = new Thickness(10),
                Orientation = Orientation.Vertical
            };

            // Set up border properties
            Background = System.Windows.Media.Brushes.MistyRose;
            BorderBrush = System.Windows.Media.Brushes.IndianRed;
            BorderThickness = new Thickness(1);
            CornerRadius = new CornerRadius(4);
            Padding = new Thickness(10);
            Margin = new Thickness(0, 0, 0, 10);

            // Create title block
            _titleBlock = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.Firebrick,
                Margin = new Thickness(0, 0, 0, 5)
            };
            _titleBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Title")
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.OneWay
            });
            panel.Children.Add(_titleBlock);

            // Create message block
            _messageBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5)
            };
            _messageBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Message")
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.OneWay
            });
            panel.Children.Add(_messageBlock);

            // Create alternative block
            _alternativeBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5),
                Visibility = Visibility.Collapsed
            };
            _alternativeBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Alternative")
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.OneWay
            });
            panel.Children.Add(_alternativeBlock);

            // Create removal date block
            _removalDateBlock = new TextBlock
            {
                FontStyle = FontStyles.Italic,
                Visibility = Visibility.Collapsed
            };
            _removalDateBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("RemovalDateText")
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.OneWay
            });
            panel.Children.Add(_removalDateBlock);

            // Set the panel as the content of the border
            Child = panel;

            // Update visibility of blocks
            UpdateAlternativeVisibility();
            UpdateRemovalDateVisibility();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == AlternativeProperty)
            {
                UpdateAlternativeVisibility();
            }
            else if (e.Property == RemovalDateProperty)
            {
                UpdateRemovalDateVisibility();
            }
        }

        private void UpdateAlternativeVisibility()
        {
            _alternativeBlock.Visibility = string.IsNullOrEmpty(Alternative)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void UpdateRemovalDateVisibility()
        {
            _removalDateBlock.Visibility = RemovalDate.HasValue
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private static void OnRemovalDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeprecatedBanner banner)
            {
                banner.UpdateRemovalDateVisibility();
            }
        }
    }
}
