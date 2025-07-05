using System;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;

namespace BusBuddy.Forms.Base
{
    /// <summary>
    /// Base class for all management forms (list/grid views) in Bus Buddy.
    /// Provides standard CRUD operations, data binding, and UI patterns.
    /// </summary>
    /// <typeparam name="T">The entity type (e.g., Bus, Driver, Route)</typeparam>
    public abstract partial class BaseManagementForm<T> : SfForm where T : class, new()
    {
        protected Syncfusion.WinForms.DataGrid.SfDataGrid? dataGrid;
        protected BindingList<T>? dataSource;
        protected ToolStrip? toolbar;
        protected ToolStripButton? btnAdd;
        protected ToolStripButton? btnEdit;
        protected ToolStripButton? btnDelete;
        protected ToolStripButton? btnRefresh;
        protected ToolStripTextBox? txtSearch;

        public BaseManagementForm()
        {
            InitializeBaseLayout();
            SetupBaseEventHandlers();
            LoadData();
        }

        protected virtual void InitializeBaseLayout()
        {
            // Toolbar
            toolbar = new ToolStrip();
            toolbar.Location = new System.Drawing.Point(0, 0);
            toolbar.Size = new System.Drawing.Size(800, 25);

            btnAdd = new ToolStripButton("Add") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btnEdit = new ToolStripButton("Edit") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btnDelete = new ToolStripButton("Delete") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btnRefresh = new ToolStripButton("Refresh") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            txtSearch = new ToolStripTextBox() { ToolTipText = "Search..." };

            toolbar.Items.AddRange(new ToolStripItem[] { btnAdd, btnEdit, btnDelete, new ToolStripSeparator(), btnRefresh, new ToolStripSeparator(), txtSearch });
            this.Controls.Add(toolbar);

            // DataGrid
            dataGrid = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            dataGrid.Location = new System.Drawing.Point(10, 35);
            dataGrid.Size = new System.Drawing.Size(800, 450);
            dataGrid.Name = "dataGrid";
            dataGrid.AllowFiltering = true;
            dataGrid.AllowSorting = true;
            dataGrid.AutoGenerateColumns = false;

            // Let derived classes configure columns
            ConfigureColumns();

            this.Controls.Add(dataGrid);

            // Set form properties
            this.Text = GetFormTitle();
            this.ClientSize = new System.Drawing.Size(820, 500);
        }

        protected virtual void SetupBaseEventHandlers()
        {
            if (btnAdd != null) btnAdd.Click += BtnAdd_Click;
            if (btnEdit != null) btnEdit.Click += BtnEdit_Click;
            if (btnDelete != null) btnDelete.Click += BtnDelete_Click;
            if (btnRefresh != null) btnRefresh.Click += BtnRefresh_Click;
            if (txtSearch != null) txtSearch.TextChanged += TxtSearch_TextChanged;
            if (dataGrid != null) dataGrid.CellDoubleClick += DataGrid_CellDoubleClick;
        }

        // Abstract methods for derived classes to implement
        protected abstract void ConfigureColumns();
        protected abstract string GetFormTitle();
        protected abstract void LoadData();
        protected abstract void AddItem();
        protected abstract void EditItem(T item);
        protected abstract bool DeleteItem(T item);

        // Event handlers
        private void BtnAdd_Click(object? sender, EventArgs e) => AddItem();

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dataGrid?.SelectedItem is T selectedItem)
                EditItem(selectedItem);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dataGrid?.SelectedItem is T selectedItem)
            {
                var result = MessageBox.Show($"Are you sure you want to delete this {typeof(T).Name}?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes && DeleteItem(selectedItem))
                {
                    dataSource?.Remove(selectedItem);
                }
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => LoadData();

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            // TODO: Implement search/filter logic
            // This would filter the dataSource based on search text
        }

        private void DataGrid_CellDoubleClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
        {
            if (dataGrid?.SelectedItem is T selectedItem)
                EditItem(selectedItem);
        }

        // Helper methods
        protected void RefreshGrid()
        {
            if (dataGrid != null && dataSource != null)
            {
                dataGrid.DataSource = null;
                dataGrid.DataSource = dataSource;
            }
        }

        protected void UpdateToolbarState()
        {
            var hasSelection = dataGrid?.SelectedItem != null;
            if (btnEdit != null) btnEdit.Enabled = hasSelection;
            if (btnDelete != null) btnDelete.Enabled = hasSelection;
        }
    }
}
