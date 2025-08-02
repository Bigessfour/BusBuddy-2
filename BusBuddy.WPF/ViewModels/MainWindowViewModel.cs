using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusBuddy.WPF.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _title = "BusBuddy - School Transportation Management";
        private bool _isLoading;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
    }
}
