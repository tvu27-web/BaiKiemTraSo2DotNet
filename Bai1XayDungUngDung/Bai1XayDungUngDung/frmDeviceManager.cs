using System.ComponentModel;
using System.Globalization;

namespace Bai1XayDungUngDung;

public partial class frmDeviceManager : Form
{
    private readonly BindingList<Device> _devices = new();
    private readonly BindingSource _bs = new();
    private readonly CultureInfo _vi = new("vi-VN");

    public frmDeviceManager()
    {
        InitializeComponent();

        ConfigureGrid();
        LoadCategories();
        SeedData();
        BindData();
        RegisterEvents();
    }

    // ===== 1. Cấu hình DataGridView bằng code =====
    private void ConfigureGrid()
    {
        dgvDevices.AutoGenerateColumns = false;
        dgvDevices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDevices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDevices.ReadOnly = true;
        dgvDevices.MultiSelect = false;
        dgvDevices.AllowUserToAddRows = false;
        dgvDevices.Columns.Clear();

        var colId = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Device.Id),
            HeaderText = "Mã TB",
            FillWeight = 12
        };
        var colName = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Device.Name),
            HeaderText = "Tên thiết bị",
            FillWeight = 35
        };
        var colCategory = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Device.Category),
            HeaderText = "Loại",
            FillWeight = 20
        };
        var colPrice = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Device.Price),
            HeaderText = "Đơn giá (VNĐ)",
            FillWeight = 20
        };
        colPrice.DefaultCellStyle.Format = "N0";
        colPrice.DefaultCellStyle.FormatProvider = _vi;
        colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        var colQty = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Device.Quantity),
            HeaderText = "Tồn kho",
            FillWeight = 13
        };
        colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        dgvDevices.Columns.AddRange(colId, colName, colCategory, colPrice, colQty);
    }

    private void LoadCategories()
    {
        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCategory.Items.Clear();
        cboCategory.Items.AddRange(new object[]
        {
            "Điện thoại", "Laptop", "Máy tính bảng", "Phụ kiện", "Màn hình"
        });
    }

    private void SeedData()
    {
        _devices.Add(new Device { Id = "TB001", Name = "iPhone 15", Category = "Điện thoại", Price = 22000000, Quantity = 10 });
        _devices.Add(new Device { Id = "TB002", Name = "Dell XPS 13", Category = "Laptop", Price = 35000000, Quantity = 5 });
        _devices.Add(new Device { Id = "TB003", Name = "Tai nghe AirPods", Category = "Phụ kiện", Price = 4500000, Quantity = 30 });
    }

    // ===== 2. Data Binding 2 chiều =====
    private void BindData()
    {
        _bs.DataSource = _devices;
        dgvDevices.DataSource = _bs;

        // Never: sửa trên form chưa ghi xuống object, chỉ ghi khi bấm "Cập nhật"
        txtDeviceId.DataBindings.Add("Text", _bs, nameof(Device.Id), true, DataSourceUpdateMode.Never);
        txtDeviceName.DataBindings.Add("Text", _bs, nameof(Device.Name), true, DataSourceUpdateMode.Never);
        cboCategory.DataBindings.Add("SelectedItem", _bs, nameof(Device.Category), true, DataSourceUpdateMode.Never);
        txtPrice.DataBindings.Add("Text", _bs, nameof(Device.Price), true,
            DataSourceUpdateMode.Never, null, "N0", _vi);
        numQuantity.DataBindings.Add("Value", _bs, nameof(Device.Quantity), true, DataSourceUpdateMode.Never);
    }

    private void RegisterEvents()
    {
        // Nếu bạn đã double-click nút trong Designer thì XÓA các dòng tương ứng ở đây
        // (nếu không sự kiện sẽ chạy 2 lần).
        btnAdd.Click += btnAdd_Click;
        btnUpdate.Click += btnUpdate_Click;
        btnDelete.Click += btnDelete_Click;
        btnClear.Click += btnClear_Click;

        // Click vào grid thì bật lại binding (sau khi bấm "Làm mới")
        dgvDevices.SelectionChanged += (s, e) =>
        {
            if (dgvDevices.SelectedRows.Count > 0)
                _bs.ResumeBinding();
        };
    }

    // ===== 3. Validation =====
    private bool TryParsePrice(out decimal price)
    {
        return decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number, _vi, out price);
    }

    private bool ValidateInput()
    {
        errorProvider.Clear();
        bool ok = true;

        if (string.IsNullOrWhiteSpace(txtDeviceName.Text))
        {
            errorProvider.SetError(txtDeviceName, "Tên thiết bị không được để trống");
            ok = false;
        }

        if (cboCategory.SelectedIndex == -1)
        {
            errorProvider.SetError(cboCategory, "Vui lòng chọn loại thiết bị");
            ok = false;
        }

        if (!TryParsePrice(out decimal price))
        {
            errorProvider.SetError(txtPrice, "Đơn giá phải là một số hợp lệ");
            ok = false;
        }
        else if (price < 0)
        {
            errorProvider.SetError(txtPrice, "Đơn giá không được nhỏ hơn 0");
            ok = false;
        }

        return ok;
    }

    // ===== 4. Các nút chức năng =====
    private string NextId()
    {
        int max = 0;
        foreach (var d in _devices)
        {
            if (d.Id.StartsWith("TB") && int.TryParse(d.Id.AsSpan(2), out int n))
                max = Math.Max(max, n);
        }
        return $"TB{max + 1:000}";
    }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput()) return;

        TryParsePrice(out decimal price);
        var device = new Device
        {
            Id = NextId(),
            Name = txtDeviceName.Text.Trim(),
            Category = cboCategory.SelectedItem!.ToString()!,
            Price = price,
            Quantity = (int)numQuantity.Value
        };

        _devices.Add(device);   // Grid tự hiện dòng mới nhờ BindingList
        ClearForm();
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        if (dgvDevices.SelectedRows.Count == 0 || _bs.Current is not Device current)
        {
            MessageBox.Show("Vui lòng chọn một thiết bị trên danh sách.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!ValidateInput()) return;

        TryParsePrice(out decimal price);
        current.Name = txtDeviceName.Text.Trim();
        current.Category = cboCategory.SelectedItem!.ToString()!;
        current.Price = price;
        current.Quantity = (int)numQuantity.Value;

        _bs.ResetCurrentItem(); // báo cho Grid vẽ lại dòng hiện tại
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvDevices.SelectedRows.Count == 0 || _bs.Current is not Device current)
        {
            MessageBox.Show("Vui lòng chọn một thiết bị để xóa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc muốn xóa thiết bị \"{current.Name}\" không?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            _bs.RemoveCurrent();
            if (_devices.Count == 0) ClearForm();
        }
    }

    private void btnClear_Click(object? sender, EventArgs e) => ClearForm();

    private void ClearForm()
    {
        _bs.SuspendBinding();           // tạm ngắt binding để ô nhập không bị đổ lại dữ liệu cũ
        errorProvider.Clear();

        txtDeviceId.Text = NextId();    // gợi ý mã kế tiếp
        txtDeviceName.Clear();
        cboCategory.SelectedIndex = -1;
        txtPrice.Clear();
        numQuantity.Value = 0;

        dgvDevices.ClearSelection();
        txtDeviceName.Focus();
    }
}