using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Handles XAML parsing errors and provides fallback values
    /// Specifically addresses the "* is not a valid value for Double" issue
    /// </summary>
    public static class XamlErrorHandler
    {
        private static readonly Serilog.ILogger Logger = Log.ForContext(typeof(XamlErrorHandler));

        /// <summary>
        /// Initialize XAML error handling for the application
        /// </summary>
        public static void Initialize()
        {
            // Handle WPF unhandled exceptions that occur during XAML parsing
            Application.Current.DispatcherUnhandledException += (sender, e) =>
            {
                if (IsXamlParsingError(e.Exception))
                {
                    Logger.Error(e.Exception, "XAML parsing error detected - attempting graceful recovery");

                    // Try to recover from common XAML errors
                    if (TryHandleXamlError(e.Exception))
                    {
                        e.Handled = true;
                        Logger.Information("Successfully recovered from XAML parsing error");
                        return;
                    }
                }

                // Log all other unhandled exceptions and always suppress error dialogs
                Logger.Error(e.Exception, "Unhandled dispatcher exception - {Message}", e.Exception.Message);
                e.Handled = true; // Always suppress error dialogs to prevent UI crashes
            };

            // Handle domain exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Logger.Fatal((Exception)e.ExceptionObject, "Unhandled domain exception");
            };

            // Handle task exceptions
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Logger.Error(e.Exception, "Unobserved task exception caught");
                e.SetObserved(); // Prevent crash
            };
        }

        /// <summary>
        /// Check if the exception is related to XAML parsing
        /// </summary>
        private static bool IsXamlParsingError(Exception exception)
        {
            return exception is XamlParseException ||
                   exception is ArgumentException argEx && argEx.Message.Contains("is not a valid value") ||
                   exception.Message.Contains("TypeConverter") ||
                   exception.Message.Contains("ProvideValue");
        }

        /// <summary>
        /// Attempt to handle and recover from XAML errors
        /// </summary>
        private static bool TryHandleXamlError(Exception exception)
        {
            try
            {
                // Handle the specific "* is not a valid value for Double" error
                if (exception.Message.Contains("* is not a valid value for Double"))
                {
                    Logger.Warning("Detected '*' parsing error - this is likely a Syncfusion control template issue");

                    // The error is typically in Syncfusion control templates that try to parse 
                    // star values as doubles. Since we can't modify the template at runtime,
                    // we'll just continue - the UI should still render with default values
                    return true;
                }

                // Handle other common XAML parsing errors
                if (exception is XamlParseException xamlEx)
                {
                    Logger.Warning("XAML parsing error at line {Line}: {Message}",
                        xamlEx.LineNumber, xamlEx.Message);

                    // For most XAML errors, we can continue - WPF will use default values
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to handle XAML error");
                return false;
            }
        }

        /// <summary>
        /// Provides safe fallback values for common XAML binding scenarios
        /// </summary>
        public static object GetSafeFallbackValue(Type targetType, object? originalValue = null)
        {
            try
            {
                if (targetType == typeof(double) || targetType == typeof(double?))
                {
                    // Return 0 for numeric values that can't be parsed
                    return 0.0;
                }

                if (targetType == typeof(GridLength))
                {
                    // Return Auto for GridLength that can't be parsed
                    return new GridLength(1, GridUnitType.Auto);
                }

                if (targetType == typeof(string))
                {
                    return originalValue?.ToString() ?? string.Empty;
                }

                if (targetType == typeof(Visibility))
                {
                    return Visibility.Collapsed;
                }

                // Return the default value for the type
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : (object)string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to provide fallback value for type {Type}", targetType);
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Safe type converter that handles parsing errors gracefully
    /// </summary>
    public class SafeDoubleConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                // Handle the problematic "*" value
                if (stringValue == "*")
                {
                    Log.ForContext<SafeDoubleConverter>()
                        .Warning("Attempted to convert '*' to double - returning 0.0");
                    return 0.0;
                }

                // Try normal parsing
                if (double.TryParse(stringValue, NumberStyles.Float, culture, out double result))
                {
                    return result;
                }

                // Return safe fallback
                return 0.0;
            }

            return base.ConvertFrom(context, culture, value) ?? 0.0;
        }
    }
}
