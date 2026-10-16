namespace Bai1XayDungUngDung
{
    partial class frmDeviceManager
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            grpInput = new GroupBox();
            numQuantity = new NumericUpDown();
            txtPrice = new TextBox();
            cboCategory = new ComboBox();
            txtDeviceName = new TextBox();
            txtDeviceId = new TextBox();
            grpActions = new GroupBox();
            btnClear = new Button();
            btnDelete = new Button();
            btnUpdate = new Button();
            btnAdd = new Button();
            dgvDevices = new DataGridView();
            errorProvider = new ErrorProvider(components);
            grpInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numQuantity).BeginInit();
            grpActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDevices).BeginInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider).BeginInit();
            SuspendLayout();
            // 
            // grpInput
            // 
            grpInput.Controls.Add(numQuantity);
            grpInput.Controls.Add(txtPrice);
            grpInput.Controls.Add(cboCategory);
            grpInput.Controls.Add(txtDeviceName);
            grpInput.Controls.Add(txtDeviceId);
            grpInput.Dock = DockStyle.Top;
            grpInput.Location = new Point(0, 0);
            grpInput.Name = "grpInput";
            grpInput.Size = new Size(882, 150);
            grpInput.TabIndex = 0;
            grpInput.TabStop = false;
            grpInput.Text = "thông tin thiết bị";
            // 
            // numQuantity
            // 
            numQuantity.Location = new Point(466, 64);
            numQuantity.Name = "numQuantity";
            numQuantity.Size = new Size(150, 27);
            numQuantity.TabIndex = 4;
            // 
            // txtPrice
            // 
            txtPrice.Location = new Point(466, 31);
            txtPrice.Name = "txtPrice";
            txtPrice.Size = new Size(125, 27);
            txtPrice.TabIndex = 3;
            txtPrice.Text = "đơn giá(VNĐ)";
            txtPrice.TextAlign = HorizontalAlignment.Right;
            // 
            // cboCategory
            // 
            cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategory.FormattingEnabled = true;
            cboCategory.Location = new Point(42, 97);
            cboCategory.Name = "cboCategory";
            cboCategory.Size = new Size(151, 28);
            cboCategory.TabIndex = 2;
            // 
            // txtDeviceName
            // 
            txtDeviceName.Location = new Point(42, 64);
            txtDeviceName.Name = "txtDeviceName";
            txtDeviceName.Size = new Size(125, 27);
            txtDeviceName.TabIndex = 1;
            txtDeviceName.Text = "tên thiết bị:";
            // 
            // txtDeviceId
            // 
            txtDeviceId.Location = new Point(42, 31);
            txtDeviceId.Name = "txtDeviceId";
            txtDeviceId.ReadOnly = true;
            txtDeviceId.Size = new Size(125, 27);
            txtDeviceId.TabIndex = 0;
            txtDeviceId.Text = "mã thiết bị:";
            // 
            // grpActions
            // 
            grpActions.BackgroundImageLayout = ImageLayout.Stretch;
            grpActions.Controls.Add(btnClear);
            grpActions.Controls.Add(btnDelete);
            grpActions.Controls.Add(btnUpdate);
            grpActions.Controls.Add(btnAdd);
            grpActions.Dock = DockStyle.Top;
            grpActions.Location = new Point(0, 150);
            grpActions.Name = "grpActions";
            grpActions.Size = new Size(882, 65);
            grpActions.TabIndex = 1;
            grpActions.TabStop = false;
            grpActions.Text = "chức năng";
            // 
            // btnClear
            // 
            btnClear.Location = new Point(658, 23);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(120, 35);
            btnClear.TabIndex = 3;
            btnClear.Text = "Làm mới";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(463, 23);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(120, 35);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Xóa";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(268, 23);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(120, 35);
            btnUpdate.TabIndex = 1;
            btnUpdate.Text = "Cập nhật";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(73, 23);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(120, 35);
            btnAdd.TabIndex = 0;
            btnAdd.TabStop = false;
            btnAdd.Text = "Thêm mới";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // dgvDevices
            // 
            dgvDevices.AllowUserToAddRows = false;
            dgvDevices.AllowUserToDeleteRows = false;
            dgvDevices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDevices.Dock = DockStyle.Fill;
            dgvDevices.Location = new Point(0, 215);
            dgvDevices.MultiSelect = false;
            dgvDevices.Name = "dgvDevices";
            dgvDevices.ReadOnly = true;
            dgvDevices.RowHeadersVisible = false;
            dgvDevices.RowHeadersWidth = 51;
            dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDevices.Size = new Size(882, 338);
            dgvDevices.TabIndex = 2;
            // 
            // errorProvider
            // 
            errorProvider.ContainerControl = this;
            // 
            // frmDeviceManager
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 553);
            Controls.Add(dgvDevices);
            Controls.Add(grpActions);
            Controls.Add(grpInput);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "frmDeviceManager";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản Lý Thiết Bị Điện Tử";
            grpInput.ResumeLayout(false);
            grpInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numQuantity).EndInit();
            grpActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDevices).EndInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpInput;
        private GroupBox grpActions;
        private DataGridView dgvDevices;
        private ComboBox cboCategory;
        private TextBox txtDeviceName;
        private TextBox txtDeviceId;
        private NumericUpDown numQuantity;
        private TextBox txtPrice;
        private Button btnClear;
        private Button btnDelete;
        private Button btnUpdate;
        private Button btnAdd;
        private ErrorProvider errorProvider;
    }
}
