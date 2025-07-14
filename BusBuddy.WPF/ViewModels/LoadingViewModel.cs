using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace BusBuddy.WPF.ViewModels
{
    public class LoadingViewModel : BaseViewModel
    {
        private string _status = "Initializing...";
        private int _progressPercentage = 0;
        private bool _isIndeterminate = true;
        private bool _isComplete = false;
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

                    // Update IsComplete when progress reaches 100%
                    if (value >= 100 && !_isComplete)
                    {
                        _isComplete = true;
                        OnPropertyChanged(nameof(IsComplete));
                        _logger.LogInformation("✅ Application initialization completed - ready for use");
                    }
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

        /// <summary>
        /// Indicates if the initialization process is complete and UI is ready
        /// </summary>
        public bool IsComplete
        {
            get => _isComplete;
            private set
            {
                if (_isComplete != value)
                {
                    _isComplete = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Event fired when initialization is complete
        /// </summary>
        public event EventHandler? InitializationCompleted;

        /// <summary>
        /// Reset the loading state for a new initialization sequence
        /// </summary>
        public void Reset()
        {
            ProgressPercentage = 0;
            Status = "Initializing...";
            IsIndeterminate = true;
            _isComplete = false;
            OnPropertyChanged(nameof(IsComplete));
            _logger.LogInformation("🔄 LoadingViewModel reset for new initialization sequence");
        }

        /// <summary>
        /// Mark initialization as complete and fire completion event
        /// </summary>
        public void CompleteInitialization()
        {
            ProgressPercentage = 100;
            Status = "Application ready!";
            IsIndeterminate = false;
            InitializationCompleted?.Invoke(this, EventArgs.Empty);
        }

        protected override ILogger? GetLogger() => _logger;
    }
}
