using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Microsoft.Extensions.Logging;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Enhanced Base Management Form that provides standardized functionality
    /// while preserving existing hard-earned methods and sophisticated implementations
    /// This class enhances rather than replaces existing management forms
    /// </summary>
    public abstract class EnhancedManagementFormBase : MetroForm
    {
        protected ILogger Logger { get; private set; }
        protected Label? StatusLabel { get; set; }
        protected SfDataGrid? DataGrid { get; set; }
        protected Button? RefreshButton { get; set; }

        // Enhanced status management
        protected virtual Color SuccessColor => Color.FromArgb(46, 204, 113);
        protected virtual Color ErrorColor => Color.FromArgb(231, 76, 60);
        protected virtual Color WarningColor => Color.FromArgb(255, 152, 0);
        protected virtual Color InfoColor => Color.FromArgb(52, 152, 219);

        protected EnhancedManagementFormBase(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Applies enhanced standardized configuration to existing form
        /// Call this from existing forms to gain enhanced functionality
        /// </summary>
        protected virtual void ApplyEnhancedConfiguration()
        {
            try
            {
                // Apply visual enhancements without breaking existing functionality
                ApplyEnhancedVisualTheme();
                ConfigureEnhancedDataGrid();
                SetupEnhancedEventHandlers();
                EnableEnhancedFontRendering();

                Logger.LogInformation("Enhanced configuration applied to {FormType}", this.GetType().Name);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not apply all enhanced configurations to {FormType}", this.GetType().Name);
            }
        }

        /// <summary>
        /// Applies enhanced visual theming that preserves existing styling
        /// </summary>
        protected virtual void ApplyEnhancedVisualTheme()
        {
            try
            {
                // Set Office2016 visual style if available
                Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(this, Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);

                // Enhanced MetroForm styling
                this.MetroColor = Color.FromArgb(46, 125, 185);
                this.CaptionBarColor = Color.FromArgb(46, 125, 185);
                this.CaptionForeColor = Color.White;

                Logger.LogDebug("Enhanced visual theme applied to {FormType}", this.GetType().Name);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Could not apply Office2016 theme to {FormType}, using enhanced fallback", this.GetType().Name);
            }
        }

        /// <summary>
        /// Configures enhanced data grid settings while preserving existing configuration
        /// </summary>
        protected virtual void ConfigureEnhancedDataGrid()
        {
            if (DataGrid == null) return;

            try
            {
                // Apply enhanced grid settings that don't conflict with existing configuration
                DataGrid.AllowFiltering = true;
                DataGrid.AllowSorting = true;
                DataGrid.AllowResizingColumns = true;
                DataGrid.ShowRowHeader = false;

                // Enhanced visual styling
                DataGrid.Style.BorderColor = Color.FromArgb(227, 227, 227);
                DataGrid.Style.BorderStyle = BorderStyle.FixedSingle;

                // Enhanced header styling
                if (DataGrid.TableSummaryRows.Count == 0)
                {
                    DataGrid.Style.HeaderStyle.BackColor = Color.FromArgb(240, 240, 240);
                    DataGrid.Style.HeaderStyle.TextColor = Color.FromArgb(68, 68, 68);
                    // Note: FontWeight property not available in current Syncfusion version
                    // DataGrid.Style.HeaderStyle.Font.FontWeight = FontWeight.SemiBold;
                    DataGrid.Style.HeaderStyle.Font.Bold = true; // Use Bold property instead
                }

                Logger.LogDebug("Enhanced data grid configuration applied to {FormType}", this.GetType().Name);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Could not apply all enhanced data grid settings to {FormType}", this.GetType().Name);
            }
        }

        /// <summary>
        /// Sets up enhanced event handlers that complement existing functionality
        /// </summary>
        protected virtual void SetupEnhancedEventHandlers()
        {
            try
            {
                // Enhanced form events
                this.Load += OnEnhancedFormLoad;
                this.Shown += OnEnhancedFormShown;

                // Enhanced data grid events
                if (DataGrid != null)
                {
                    DataGrid.QueryRowHeight += OnEnhancedQueryRowHeight;
                }

                Logger.LogDebug("Enhanced event handlers setup for {FormType}", this.GetType().Name);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Could not setup all enhanced event handlers for {FormType}", this.GetType().Name);
            }
        }

        /// <summary>
        /// Enables high-quality font rendering
        /// </summary>
        protected virtual void EnableEnhancedFontRendering()
        {
            try
            {
                this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                             ControlStyles.UserPaint |
                             ControlStyles.DoubleBuffer |
                             ControlStyles.ResizeRedraw, true);
                this.UpdateStyles();

                Logger.LogDebug("Enhanced font rendering enabled for {FormType}", this.GetType().Name);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Could not enable enhanced font rendering for {FormType}", this.GetType().Name);
            }
        }

        /// <summary>
        /// Enhanced status management that preserves existing status functionality
        /// </summary>
        protected virtual void SetEnhancedStatus(string message, StatusType statusType = StatusType.Info)
        {
            if (StatusLabel == null) return;

            try
            {
                StatusLabel.Text = message;
                StatusLabel.ForeColor = statusType switch
                {
                    StatusType.Success => SuccessColor,
                    StatusType.Error => ErrorColor,
                    StatusType.Warning => WarningColor,
                    StatusType.Info => InfoColor,
                    _ => InfoColor
                };

                Logger.LogDebug("Enhanced status set: {Message} ({StatusType})", message, statusType);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Could not set enhanced status for {FormType}", this.GetType().Name);
            }
        }

        /// <summary>
        /// Enhanced async operation wrapper that provides consistent error handling
        /// </summary>
        protected virtual async Task ExecuteEnhancedAsyncOperation(Func<Task> operation, string operationName,
            Button? targetButton = null, string? successMessage = null)
        {
            try
            {
                // Visual feedback
                SetEnhancedStatus($"Executing {operationName}...", StatusType.Info);

                if (targetButton != null)
                {
                    var originalText = targetButton.Text;
                    targetButton.Enabled = false;
                    targetButton.Text = "Working...";

                    try
                    {
                        await operation();

                        if (!string.IsNullOrEmpty(successMessage))
                        {
                            SetEnhancedStatus(successMessage, StatusType.Success);
                        }
                    }
                    finally
                    {
                        targetButton.Enabled = true;
                        targetButton.Text = originalText;
                    }
                }
                else
                {
                    await operation();

                    if (!string.IsNullOrEmpty(successMessage))
                    {
                        SetEnhancedStatus(successMessage, StatusType.Success);
                    }
                }

                Logger.LogInformation("Enhanced async operation completed: {OperationName}", operationName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Enhanced async operation failed: {OperationName}", operationName);
                SetEnhancedStatus($"Error in {operationName}: {ex.Message}", StatusType.Error);

                MessageBox.Show($"Error in {operationName}: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Enhanced data refresh method that works with existing refresh implementations
        /// </summary>
        protected virtual async Task RefreshEnhancedDataAsync()
        {
            await ExecuteEnhancedAsyncOperation(
                async () => await OnRefreshDataAsync(),
                "data refresh",
                RefreshButton,
                "Data refreshed successfully"
            );
        }

        /// <summary>
        /// Override this method in derived classes to implement specific refresh logic
        /// </summary>
        protected abstract Task OnRefreshDataAsync();

        #region Enhanced Event Handlers

        protected virtual void OnEnhancedFormLoad(object? sender, EventArgs e)
        {
            Logger.LogDebug("Enhanced form load event for {FormType}", this.GetType().Name);
        }

        protected virtual void OnEnhancedFormShown(object? sender, EventArgs e)
        {
            Logger.LogDebug("Enhanced form shown event for {FormType}", this.GetType().Name);
        }

        protected virtual void OnEnhancedQueryRowHeight(object? sender, Syncfusion.WinForms.DataGrid.Events.QueryRowHeightEventArgs e)
        {
            // Enhanced row height for better readability
            if (e.RowIndex > 0) // Skip header
            {
                e.Height = Math.Max(e.Height, 28); // Minimum row height for better UX
                e.Handled = true;
            }
        }

        #endregion

        #region Enhanced Helper Methods

        /// <summary>
        /// Enhanced message box that uses Syncfusion styling when available
        /// </summary>
        protected virtual DialogResult ShowEnhancedMessage(string message, string title,
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            try
            {
                // Try to use Syncfusion MessageBoxAdv if available
                return Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, title, buttons, icon);
            }
            catch
            {
                // Fallback to standard MessageBox
                return MessageBox.Show(message, title, buttons, icon);
            }
        }

        /// <summary>
        /// Enhanced validation that can be used by existing forms
        /// </summary>
        protected virtual bool ValidateEnhancedForm()
        {
            try
            {
                return OnValidateForm();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Enhanced form validation failed for {FormType}", this.GetType().Name);
                SetEnhancedStatus("Form validation failed", StatusType.Error);
                return false;
            }
        }

        /// <summary>
        /// Override this method in derived classes to implement specific validation logic
        /// </summary>
        protected virtual bool OnValidateForm()
        {
            return true; // Default implementation
        }

        #endregion
    }

    /// <summary>
    /// Status types for enhanced status management
    /// </summary>
    public enum StatusType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Extension methods to help existing forms adopt enhanced functionality
    /// </summary>
    public static class EnhancedManagementFormExtensions
    {
        /// <summary>
        /// Applies enhanced configuration to existing management forms
        /// </summary>
        public static void ApplyEnhancedFeatures(this MetroForm form, ILogger logger,
            SfDataGrid? dataGrid = null, Label? statusLabel = null, Button? refreshButton = null)
        {
            try
            {
                // Apply enhanced visual theming
                try
                {
                    Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(form, Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);
                    form.MetroColor = Color.FromArgb(46, 125, 185);
                    form.CaptionBarColor = Color.FromArgb(46, 125, 185);
                    form.CaptionForeColor = Color.White;
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Could not apply Office2016 theme to {FormType}", form.GetType().Name);
                }

                // Apply enhanced data grid configuration
                if (dataGrid != null)
                {
                    try
                    {
                        dataGrid.AllowFiltering = true;
                        dataGrid.AllowSorting = true;
                        dataGrid.AllowResizingColumns = true;
                        dataGrid.ShowRowHeader = false;
                        dataGrid.Style.BorderColor = Color.FromArgb(227, 227, 227);
                        dataGrid.Style.BorderStyle = BorderStyle.FixedSingle;
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(ex, "Could not apply enhanced data grid configuration to {FormType}", form.GetType().Name);
                    }
                }

                // Enable high-quality font rendering
                try
                {
                    // Note: SetStyle, UpdateStyles, and DoubleBuffered are protected members
                    // These optimizations should be applied within each form's constructor
                    // For now, we'll skip these optimizations to avoid compilation errors
                    System.Diagnostics.Debug.WriteLine("Font rendering optimizations skipped due to access restrictions");
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Could not enable enhanced font rendering for {FormType}", form.GetType().Name);
                }

                logger.LogInformation("Enhanced features applied to existing form {FormType}", form.GetType().Name);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not apply all enhanced features to {FormType}", form.GetType().Name);
            }
        }

        /// <summary>
        /// Sets enhanced status on existing forms
        /// </summary>
        public static void SetEnhancedStatus(this Label statusLabel, string message, StatusType statusType = StatusType.Info)
        {
            if (statusLabel == null) return;

            statusLabel.Text = message;
            statusLabel.ForeColor = statusType switch
            {
                StatusType.Success => Color.FromArgb(46, 204, 113),
                StatusType.Error => Color.FromArgb(231, 76, 60),
                StatusType.Warning => Color.FromArgb(255, 152, 0),
                StatusType.Info => Color.FromArgb(52, 152, 219),
                _ => Color.FromArgb(52, 152, 219)
            };
        }
    }
}
