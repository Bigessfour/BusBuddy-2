using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.ListView;
using Syncfusion.WinForms.Input;

namespace Bus_Buddy.Forms
{
    partial class FuelEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        // private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainPanel = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.titleLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.formTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.busLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.busComboBox = new Syncfusion.WinForms.ListView.SfComboBox();
            this.dateLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.dateEdit = new Syncfusion.WinForms.Input.SfDateTimeEdit();
            this.odometerLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.odometerTextBox = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.fuelTypeLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.fuelTypeComboBox = new Syncfusion.WinForms.ListView.SfComboBox();
            this.gallonsLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.gallonsTextBox = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.pricePerGallonLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.pricePerGallonTextBox = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.costLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.costTextBox = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.locationLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.locationTextBox = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.notesLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.notesTextBox = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.buttonPanel = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.saveButton = new Syncfusion.WinForms.Controls.SfButton();
            this.cancelButton = new Syncfusion.WinForms.Controls.SfButton();
            ((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
            this.mainPanel.SuspendLayout();
            this.formTableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.busComboBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fuelTypeComboBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.odometerTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gallonsTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pricePerGallonTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.costTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.locationTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.notesTextBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonPanel)).BeginInit();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mainPanel.Controls.Add(this.titleLabel);
            this.mainPanel.Controls.Add(this.formTableLayout);
            this.mainPanel.Controls.Add(this.buttonPanel);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(634, 541);
            this.mainPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(114)))), ((int)(((byte)(196)))));
            this.titleLabel.Location = new System.Drawing.Point(20, 20);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(123, 30);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Title Label";
            // 
            // formTableLayout
            // 
            this.formTableLayout.ColumnCount = 2;
            this.formTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.formTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.formTableLayout.Controls.Add(this.busLabel, 0, 0);
            this.formTableLayout.Controls.Add(this.busComboBox, 1, 0);
            this.formTableLayout.Controls.Add(this.dateLabel, 0, 1);
            this.formTableLayout.Controls.Add(this.dateEdit, 1, 1);
            this.formTableLayout.Controls.Add(this.odometerLabel, 0, 2);
            this.formTableLayout.Controls.Add(this.odometerTextBox, 1, 2);
            this.formTableLayout.Controls.Add(this.fuelTypeLabel, 0, 3);
            this.formTableLayout.Controls.Add(this.fuelTypeComboBox, 1, 3);
            this.formTableLayout.Controls.Add(this.gallonsLabel, 0, 4);
            this.formTableLayout.Controls.Add(this.gallonsTextBox, 1, 4);
            this.formTableLayout.Controls.Add(this.pricePerGallonLabel, 0, 5);
            this.formTableLayout.Controls.Add(this.pricePerGallonTextBox, 1, 5);
            this.formTableLayout.Controls.Add(this.costLabel, 0, 6);
            this.formTableLayout.Controls.Add(this.costTextBox, 1, 6);
            this.formTableLayout.Controls.Add(this.locationLabel, 0, 7);
            this.formTableLayout.Controls.Add(this.locationTextBox, 1, 7);
            this.formTableLayout.Controls.Add(this.notesLabel, 0, 8);
            this.formTableLayout.Controls.Add(this.notesTextBox, 1, 8);
            this.formTableLayout.Location = new System.Drawing.Point(20, 60);
            this.formTableLayout.Name = "formTableLayout";
            this.formTableLayout.RowCount = 9;
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.11111F));
            this.formTableLayout.Size = new System.Drawing.Size(580, 400);
            this.formTableLayout.TabIndex = 1;
            // 
            // busLabel
            // 
            this.busLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.busLabel.AutoSize = true;
            this.busLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.busLabel.Location = new System.Drawing.Point(3, 12);
            this.busLabel.Name = "busLabel";
            this.busLabel.Size = new System.Drawing.Size(33, 19);
            this.busLabel.TabIndex = 0;
            this.busLabel.Text = "Bus:";
            // 
            // busComboBox
            // 
            this.busComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.busComboBox.DropDownStyle = Syncfusion.WinForms.ListView.Enums.DropDownStyle.DropDownList;
            this.busComboBox.Location = new System.Drawing.Point(153, 11);
            this.busComboBox.Name = "busComboBox";
            this.busComboBox.Size = new System.Drawing.Size(424, 21);
            this.busComboBox.TabIndex = 1;
            // 
            // dateLabel
            // 
            this.dateLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateLabel.AutoSize = true;
            this.dateLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dateLabel.Location = new System.Drawing.Point(3, 56);
            this.dateLabel.Name = "dateLabel";
            this.dateLabel.Size = new System.Drawing.Size(40, 19);
            this.dateLabel.TabIndex = 2;
            this.dateLabel.Text = "Date:";
            // 
            // dateEdit
            // 
            this.dateEdit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateEdit.Location = new System.Drawing.Point(153, 55);
            this.dateEdit.Name = "dateEdit";
            this.dateEdit.Size = new System.Drawing.Size(200, 21);
            this.dateEdit.TabIndex = 3;
            // 
            // odometerLabel
            // 
            this.odometerLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.odometerLabel.AutoSize = true;
            this.odometerLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.odometerLabel.Location = new System.Drawing.Point(3, 100);
            this.odometerLabel.Name = "odometerLabel";
            this.odometerLabel.Size = new System.Drawing.Size(74, 19);
            this.odometerLabel.TabIndex = 4;
            this.odometerLabel.Text = "Odometer:";
            // 
            // odometerTextBox
            // 
            this.odometerTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.odometerTextBox.Location = new System.Drawing.Point(153, 99);
            this.odometerTextBox.Name = "odometerTextBox";
            this.odometerTextBox.Size = new System.Drawing.Size(120, 20);
            this.odometerTextBox.TabIndex = 5;
            // 
            // fuelTypeLabel
            // 
            this.fuelTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.fuelTypeLabel.AutoSize = true;
            this.fuelTypeLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.fuelTypeLabel.Location = new System.Drawing.Point(3, 144);
            this.fuelTypeLabel.Name = "fuelTypeLabel";
            this.fuelTypeLabel.Size = new System.Drawing.Size(70, 19);
            this.fuelTypeLabel.TabIndex = 6;
            this.fuelTypeLabel.Text = "Fuel Type:";
            // 
            // fuelTypeComboBox
            // 
            this.fuelTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.fuelTypeComboBox.Location = new System.Drawing.Point(153, 143);
            this.fuelTypeComboBox.Name = "fuelTypeComboBox";
            this.fuelTypeComboBox.Size = new System.Drawing.Size(424, 21);
            this.fuelTypeComboBox.TabIndex = 7;
            // 
            // gallonsLabel
            // 
            this.gallonsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.gallonsLabel.AutoSize = true;
            this.gallonsLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.gallonsLabel.Location = new System.Drawing.Point(3, 188);
            this.gallonsLabel.Name = "gallonsLabel";
            this.gallonsLabel.Size = new System.Drawing.Size(56, 19);
            this.gallonsLabel.TabIndex = 8;
            this.gallonsLabel.Text = "Gallons:";
            // 
            // gallonsTextBox
            // 
            this.gallonsTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.gallonsTextBox.Location = new System.Drawing.Point(153, 187);
            this.gallonsTextBox.Name = "gallonsTextBox";
            this.gallonsTextBox.Size = new System.Drawing.Size(120, 20);
            this.gallonsTextBox.TabIndex = 9;
            // 
            // pricePerGallonLabel
            // 
            this.pricePerGallonLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pricePerGallonLabel.AutoSize = true;
            this.pricePerGallonLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.pricePerGallonLabel.Location = new System.Drawing.Point(3, 232);
            this.pricePerGallonLabel.Name = "pricePerGallonLabel";
            this.pricePerGallonLabel.Size = new System.Drawing.Size(108, 19);
            this.pricePerGallonLabel.TabIndex = 10;
            this.pricePerGallonLabel.Text = "Price Per Gallon:";
            // 
            // pricePerGallonTextBox
            // 
            this.pricePerGallonTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pricePerGallonTextBox.Location = new System.Drawing.Point(153, 231);
            this.pricePerGallonTextBox.Name = "pricePerGallonTextBox";
            this.pricePerGallonTextBox.Size = new System.Drawing.Size(120, 20);
            this.pricePerGallonTextBox.TabIndex = 11;
            // 
            // costLabel
            // 
            this.costLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.costLabel.AutoSize = true;
            this.costLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.costLabel.Location = new System.Drawing.Point(3, 276);
            this.costLabel.Name = "costLabel";
            this.costLabel.Size = new System.Drawing.Size(40, 19);
            this.costLabel.TabIndex = 12;
            this.costLabel.Text = "Cost:";
            // 
            // costTextBox
            // 
            this.costTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.costTextBox.Location = new System.Drawing.Point(153, 275);
            this.costTextBox.Name = "costTextBox";
            this.costTextBox.Size = new System.Drawing.Size(120, 20);
            this.costTextBox.TabIndex = 13;
            // 
            // locationLabel
            // 
            this.locationLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.locationLabel.AutoSize = true;
            this.locationLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.locationLabel.Location = new System.Drawing.Point(3, 320);
            this.locationLabel.Name = "locationLabel";
            this.locationLabel.Size = new System.Drawing.Size(64, 19);
            this.locationLabel.TabIndex = 14;
            this.locationLabel.Text = "Location:";
            // 
            // locationTextBox
            // 
            this.locationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.locationTextBox.Location = new System.Drawing.Point(153, 319);
            this.locationTextBox.Name = "locationTextBox";
            this.locationTextBox.Size = new System.Drawing.Size(424, 20);
            this.locationTextBox.TabIndex = 15;
            // 
            // notesLabel
            // 
            this.notesLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.notesLabel.AutoSize = true;
            this.notesLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.notesLabel.Location = new System.Drawing.Point(3, 367);
            this.notesLabel.Name = "notesLabel";
            this.notesLabel.Size = new System.Drawing.Size(48, 19);
            this.notesLabel.TabIndex = 16;
            this.notesLabel.Text = "Notes:";
            // 
            // notesTextBox
            // 
            this.notesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.notesTextBox.Location = new System.Drawing.Point(153, 366);
            this.notesTextBox.Multiline = true;
            this.notesTextBox.Name = "notesTextBox";
            this.notesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.notesTextBox.Size = new System.Drawing.Size(424, 21);
            this.notesTextBox.TabIndex = 17;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.saveButton);
            this.buttonPanel.Controls.Add(this.cancelButton);
            this.buttonPanel.Location = new System.Drawing.Point(20, 470);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(580, 50);
            this.buttonPanel.TabIndex = 2;
            // 
            // saveButton
            // 
            this.saveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.saveButton.ForeColor = System.Drawing.Color.White;
            this.saveButton.Location = new System.Drawing.Point(240, 8);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(100, 35);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = false;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(350, 8);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(80, 35);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // FuelEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 541);
            this.Controls.Add(this.mainPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FuelEditForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FuelEditForm";
            ((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.formTableLayout.ResumeLayout(false);
            this.formTableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.busComboBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fuelTypeComboBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.odometerTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gallonsTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pricePerGallonTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.costTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.locationTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.notesTextBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonPanel)).EndInit();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel mainPanel;
        private Syncfusion.Windows.Forms.Tools.AutoLabel titleLabel;
        private System.Windows.Forms.TableLayoutPanel formTableLayout;
        private Syncfusion.Windows.Forms.Tools.AutoLabel busLabel;
        private Syncfusion.WinForms.ListView.SfComboBox busComboBox;
        private Syncfusion.Windows.Forms.Tools.AutoLabel dateLabel;
        private Syncfusion.WinForms.Input.SfDateTimeEdit dateEdit;
        private Syncfusion.Windows.Forms.Tools.AutoLabel odometerLabel;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt odometerTextBox;
        private Syncfusion.Windows.Forms.Tools.AutoLabel fuelTypeLabel;
        private Syncfusion.WinForms.ListView.SfComboBox fuelTypeComboBox;
        private Syncfusion.Windows.Forms.Tools.AutoLabel costLabel;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt costTextBox;
        private Syncfusion.Windows.Forms.Tools.AutoLabel notesLabel;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt notesTextBox;
        private Syncfusion.Windows.Forms.Tools.GradientPanel buttonPanel;
        private Syncfusion.WinForms.Controls.SfButton saveButton;
        private Syncfusion.WinForms.Controls.SfButton cancelButton;
        private AutoLabel gallonsLabel;
        private TextBoxExt gallonsTextBox;
        private AutoLabel pricePerGallonLabel;
        private TextBoxExt pricePerGallonTextBox;
        private AutoLabel locationLabel;
        private TextBoxExt locationTextBox;
    }
}
