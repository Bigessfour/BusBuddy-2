using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;

namespace BusBuddy.WPF.Views
{
    public partial class RouteManagementView : UserControl
    {
        public RouteManagementView()
        {
            InitializeComponent();
            Loaded += RouteManagementView_Loaded;
        }

        private async void RouteManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is RouteManagementViewModel vm)
            {
                // Load live routes on startup
                await LoadRoutesToMapAsync(vm);
            }
        }

        private async Task LoadRoutesToMapAsync(RouteManagementViewModel vm)
        {
            var geoJson = await vm.GetLiveRouteGeoJsonAsync("your_region");
            // Escape for JS string
            var safeGeoJson = geoJson.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");
            var js = $"window.loadGeoJson && window.loadGeoJson(JSON.parse('{safeGeoJson}'));";
            await MapWebView.ExecuteScriptAsync(js);
        }
    }
}
