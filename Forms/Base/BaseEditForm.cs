using System;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;

namespace BusBuddy.Forms.Base
{
    /// <summary>
    /// Base class for all edit forms (add/edit dialogs) in Bus Buddy.
    /// Provides standard validation, save/cancel operations, and UI patterns.
    /// </summary>
    /// <typeparam name="T">The entity type being edited</typeparam>
    public abstract partial class BaseEditForm<T> : SfForm where T : class, new()
    {
        protected T? currentItem;
        protected bool isEditMode;
        protected Button? btnSave;
        protected Button? btnCancel;
        protected TableLayoutPanel? mainPanel;

        public T? Item => currentItem;
        public bool IsEditMode => isEditMode;

        public BaseEditForm() : this(null) { }

        public BaseEditForm(T? item)
        {
            currentItem = item ?? new T();
            isEditMode = item != null;

            InitializeBaseLayout();
            SetupBaseEventHandlers();
            LoadData();
        }

        protected virtual void InitializeBaseLayout()
        {
            // Main panel for form layout
            mainPanel = new TableLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.ColumnCount = 2;
            mainPanel.RowCount = 10; // Adjust based on needs
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // Add form-specific controls
            CreateFormControls();

            this.Controls.Add(mainPanel);

            // Buttons panel
            var buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Height = 50;

            btnCancel = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 75 };
            btnSave = new Button() { Text = "Save", DialogResult = DialogResult.OK, Width = 75 };

            buttonPanel.Controls.AddRange(new Control[] { btnCancel, btnSave });
            this.Controls.Add(buttonPanel);

            // Set form properties
            this.Text = GetFormTitle();
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        protected virtual void SetupBaseEventHandlers()
        {
            if (btnSave != null) btnSave.Click += BtnSave_Click;
            if (btnCancel != null) btnCancel.Click += BtnCancel_Click;
        }

        // Abstract methods for derived classes
        protected abstract void CreateFormControls();
        protected abstract string GetFormTitle();
        protected abstract void LoadData();
        protected abstract bool ValidateInput();
        protected abstract void SaveData();

        // Event handlers
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    SaveData();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving data: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Helper methods
        protected void AddLabelAndControl(string labelText, Control control, int row)
        {
            if (mainPanel == null) return;

            var label = new Label()
            {
                Text = labelText,
                Anchor = AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight
            };

            control.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            mainPanel.Controls.Add(label, 0, row);
            mainPanel.Controls.Add(control, 1, row);
        }

        protected void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
