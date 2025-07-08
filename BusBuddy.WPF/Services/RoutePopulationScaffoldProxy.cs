using System;
using System.Threading.Tasks;
using BusBuddy.Core.Services;

namespace BusBuddy.WPF.Services
{
    public interface IRoutePopulationScaffold
    {
        Task<System.Collections.Generic.List<BusBuddy.Core.Models.Route>> GetOptimizedRoutesAsync();
    }

    public class RoutePopulationScaffoldProxy : IRoutePopulationScaffold
    {
        private readonly RoutePopulationScaffold _scaffold;
        public RoutePopulationScaffoldProxy(RoutePopulationScaffold scaffold)
        {
            _scaffold = scaffold;
        }
        public Task<System.Collections.Generic.List<BusBuddy.Core.Models.Route>> GetOptimizedRoutesAsync()
            => _scaffold.GetOptimizedRoutesAsync();
    }
}
