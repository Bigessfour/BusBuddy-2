using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.ViewModels
{
    public class LoadingViewModel : BaseViewModel
    {
        private string _status = "Initializing...";
        private int _progressPercentage;
        private bool _isIndeterminate = true;
        private readonly ILogger<LoadingViewModel> _logger;

        public LoadingViewModel(ILogger<LoadingViewModel> logger)
        {
            _logger = logger;
            _logger.LogInformation("LoadingViewModel created");
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    _logger.LogDebug("Loading status changed to: {Status}", value);
                }
            }
        }

        public int ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                if (_progressPercentage != value)
                {
                    _progressPercentage = value;
                    OnPropertyChanged();
                    _logger.LogDebug("Progress changed to: {Progress}%", value);
                }
            }
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                if (_isIndeterminate != value)
                {
                    _isIndeterminate = value;
                    OnPropertyChanged();
                }
            }
        }

        protected override ILogger? GetLogger() => _logger;
    }
}
