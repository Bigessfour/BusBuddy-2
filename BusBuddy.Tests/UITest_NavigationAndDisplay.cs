using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.Tests
{
    [TestClass]
    public class UITest_NavigationAndDisplay
    {
        [TestMethod]
        public async Task DriversViewModel_LoadsDrivers()
        {
            var vm = new DriversViewModel();
            await vm.LoadDriversAsync();
            Assert.IsTrue(vm.Drivers.Count > 0, "Drivers should be loaded and displayed.");
        }

        [TestMethod]
        public async Task VehiclesViewModel_LoadsVehicles()
        {
            var vm = new VehiclesViewModel();
            await vm.LoadVehiclesAsync();
            Assert.IsTrue(vm.Vehicles.Count > 0, "Vehicles should be loaded and displayed.");
        }

        [TestMethod]
        public async Task ActivityScheduleViewModel_LoadsActivities()
        {
            var vm = new ActivityScheduleViewModel();
            await vm.LoadActivitiesAsync();
            Assert.IsTrue(vm.Activities.Count > 0, "Activities should be loaded and displayed.");
        }
    }
}
