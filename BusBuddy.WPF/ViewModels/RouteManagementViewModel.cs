using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusBuddy.Core.Services;

namespace BusBuddy.WPF.ViewModels
{
    public class RouteManagementViewModel : ObservableObject
    {
        private readonly GoogleEarthEngineService _geeService;
        public ICommand LoadRoutesCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }

        public RouteManagementViewModel(GoogleEarthEngineService geeService)
        {
            _geeService = geeService;
            LoadRoutesCommand = new AsyncRelayCommand(LoadRoutesAsync);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
        }

        // These methods can be used to communicate with the WebView2 control via JS interop

        public async Task<string> GetLiveRouteGeoJsonAsync(string regionOrAsset)
        {
            return await _geeService.GetRouteGeoJsonAsync(regionOrAsset);
        }

        private async Task LoadRoutesAsync()
        {
            // This method is now handled in the view for JS interop
            await Task.CompletedTask;
        }

        private void ZoomIn()
        {
            // Call JS interop to zoom in map in WebView2 (handled in code-behind)
        }

        private void ZoomOut()
        {
            // Call JS interop to zoom out map in WebView2 (handled in code-behind)
        }
    }
}
