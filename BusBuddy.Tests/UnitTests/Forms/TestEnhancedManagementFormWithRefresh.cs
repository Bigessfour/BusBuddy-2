using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Forms;

namespace BusBuddy.Tests.UnitTests.Forms
{
    // Minimal stub for abstract OnRefreshDataAsync
    public class TestEnhancedManagementFormWithRefresh : EnhancedManagementFormBase
    {
        public TestEnhancedManagementFormWithRefresh(ILogger logger) : base(logger) { }
        public new void ApplyEnhancedConfiguration() => base.ApplyEnhancedConfiguration();
        public new async Task ExecuteEnhancedAsyncOperation(Func<Task> operation, string operationName, Button? targetButton = null, string? successMessage = null)
            => await base.ExecuteEnhancedAsyncOperation(operation, operationName, targetButton, successMessage);
        public new Label? StatusLabel { get => base.StatusLabel; set => base.StatusLabel = value; }
        protected override void ApplyEnhancedVisualTheme() { }
        protected override void ConfigureEnhancedDataGrid() { }
        protected override void SetupEnhancedEventHandlers() { }
        protected override void EnableEnhancedFontRendering() { }
        protected override Task OnRefreshDataAsync() => Task.CompletedTask;
    }
}
