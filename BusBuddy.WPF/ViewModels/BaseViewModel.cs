using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.ViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _errorMessage = null;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        protected virtual ILogger? GetLogger() => null;

        protected async Task LoadDataAsync(Func<Task> loadAction)
        {
            var logger = GetLogger();
            try
            {
                logger?.LogDebug("LoadDataAsync started");
                IsLoading = true;
                ErrorMessage = null;
                await loadAction();
                logger?.LogDebug("LoadDataAsync completed successfully");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error in LoadDataAsync: {0}", ex.Message);
                ErrorMessage = $"Failed to load data: {ex.Message}";

                // Capture and log inner exception details if available
                if (ex.InnerException != null)
                {
                    logger?.LogError(ex.InnerException, "Inner exception: {0}", ex.InnerException.Message);
                    ErrorMessage += $" (Inner: {ex.InnerException.Message})";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
