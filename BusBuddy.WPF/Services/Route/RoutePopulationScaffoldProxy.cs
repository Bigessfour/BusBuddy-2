using BusBuddy.Core.Services;
using BusBuddy.WPF.Services;

namespace BusBuddy.WPF.Services
{
    public class RoutePopulationScaffoldProxy : IRoutePopulationScaffold
    {
        private readonly RoutePopulationScaffold _scaffold;
        public RoutePopulationScaffoldProxy(RoutePopulationScaffold scaffold)
        {
            _scaffold = scaffold;
        }
        public Task<System.Collections.Generic.List<BusBuddy.Core.Models.Route>> GetOptimizedRoutesAsync()
            => _scaffold.GetOptimizedRoutesAsync();

        public Task PopulateRoutesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
