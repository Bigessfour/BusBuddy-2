using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// Base class for view models that are in development status
    /// This provides common properties for showing placeholder content
    /// </summary>
    public abstract class BaseInDevelopmentViewModel : BaseViewModel
    {
        protected readonly ILogger? Logger;
        private bool _isInDevelopment = true;

        protected BaseInDevelopmentViewModel(ILogger? logger = null)
        {
            Logger = logger;
            Logger?.LogInformation($"{GetType().Name} initialized in development mode");
        }

        /// <summary>
        /// Indicates whether this module is in development mode
        /// Controls visibility of placeholder overlay in views
        /// </summary>
        public bool IsInDevelopment
        {
            get => _isInDevelopment;
            set => SetProperty(ref _isInDevelopment, value);
        }

        protected override ILogger? GetLogger() => Logger;
    }
}
