using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Bai3QuanLySieuThi;

// Kho dữ liệu dùng chung: đóng form rồi mở lại vẫn còn dữ liệu cũ
internal static class DataStore
{
    public static BindingList<Product> Products { get; } = new()
    {
        new Product { Id = "SP001", Name = "Sữa tươi 1L", Category = "Đồ uống", Price = 32000, Quantity = 120 },
        new Product { Id = "SP002", Name = "Mì gói", Category = "Thực phẩm", Price = 5000, Quantity = 500 },
        new Product { Id = "SP003", Name = "Nước giặt 3kg", Category = "Gia dụng", Price = 145000, Quantity = 40 }
    };
}

// Form con 1: gộp Bài 1 (CRUD + Validation + Binding) và Bài 2 (ảnh + CSV)
public class frmProductCatalog : Form
{
    private readonly BindingList<Product> _products = DataStore.Products;
    private readonly BindingSource _bs = new();
    private readonly CultureInfo _vi = new("vi-VN");
    private string _imagePath = "";

    private readonly GroupBox _grpInput = new();
    private readonly TextBox txtProductId = new();
    private readonly TextBox txtProductName = new();
    private readonly ComboBox cboCategory = new();
    private readonly TextBox txtPrice = new();
    private readonly NumericUpDown numQuantity = new();
    private readonly TextBox txtImagePath = new();
    private readonly PictureBox picProductImage = new();
    private readonly Button btnChooseImage = new();
    private readonly Button btnAdd = new();
    private readonly Button btnUpdate = new();
    private readonly Button btnDelete = new();
    private readonly Button btnClear = new();
    private readonly Button btnImport = new();
    private readonly Button btnExport = new();
    private readonly Label lblTotals = new();
    private readonly DataGridView dgvProducts = new();
    private readonly ErrorProvider errorProvider = new();

    public frmProductCatalog()
    {
        Text = "Quản lý Sản phẩm";
        ClientSize = new Size(960, 560);

        BuildInputGroup();

        lblTotals.Dock = DockStyle.Bottom;
        lblTotals.Height = 28;
        lblTotals.TextAlign = ContentAlignment.MiddleLeft;
        lblTotals.Padding = new Padding(8, 0, 0, 0);
        dgvProducts.Dock = DockStyle.Fill;

        // Fill add trước, Top/Bottom add sau (quy tắc Dock)
        Controls.Add(dgvProducts);
        Controls.Add(lblTotals);
        Controls.Add(_grpInput);

        ConfigureGrid();
        _bs.DataSource = _products;
        dgvProducts.DataSource = _bs;
        _bs.ListChanged += (s, e) => UpdateTotals();

        UpdateTotals();
        ClearForm();
    }

    // ===================== GIAO DIỆN =====================
    private void AddField(string caption, Control ctl, int labelX, int y, int width = 200)
    {
        _grpInput.Controls.Add(new Label { Text = caption, Left = labelX, Top = y + 4, AutoSize = true });
        ctl.Left = labelX + 105;
        ctl.Top = y;
        ctl.Width = width;
        _grpInput.Controls.Add(ctl);
    }

    private void BuildInputGroup()
    {
        _grpInput.Text = "Thông tin sản phẩm";
        _grpInput.Dock = DockStyle.Top;
        _grpInput.Height = 220;

        txtProductId.ReadOnly = true;
        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCategory.Items.AddRange(new object[] { "Thực phẩm", "Đồ uống", "Gia dụng", "Mỹ phẩm", "Khác" });
        txtPrice.TextAlign = HorizontalAlignment.Right;
        numQuantity.Minimum = 0;
        numQuantity.Maximum = 1_000_000;
        txtImagePath.ReadOnly = true;

        AddField("Mã sản phẩm:", txtProductId, 15, 30);
        AddField("Tên sản phẩm:", txtProductName, 15, 65);
        AddField("Loại sản phẩm:", cboCategory, 15, 100);
        AddField("Đơn giá (VNĐ):", txtPrice, 15, 135);
        AddField("Số lượng tồn:", numQuantity, 340, 30, 220);
        AddField("Đường dẫn ảnh:", txtImagePath, 340, 65, 220);

        picProductImage.SetBounds(740, 25, 190, 140);
        picProductImage.BorderStyle = BorderStyle.FixedSingle;
        picProductImage.SizeMode = PictureBoxSizeMode.Zoom;
        picProductImage.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnChooseImage.Text = "Chọn ảnh";
        btnChooseImage.SetBounds(740, 172, 190, 32);
        btnChooseImage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnChooseImage.Click += (s, e) => ChooseImage();

        string[] captions = { "Thêm mới", "Cập nhật", "Xóa", "Làm mới", "Import CSV", "Export CSV" };
        Button[] buttons = { btnAdd, btnUpdate, btnDelete, btnClear, btnImport, btnExport };
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].Text = captions[i];
            buttons[i].SetBounds(120 + i * 105, 172, 98, 32);
        }
        btnAdd.Click += (s, e) => AddProduct();
        btnUpdate.Click += (s, e) => UpdateProduct();
        btnDelete.Click += (s, e) => DeleteProduct();
        btnClear.Click += (s, e) => ClearForm();
        btnImport.Click += (s, e) => ImportCsv();
        btnExport.Click += (s, e) => ExportCsv();

        _grpInput.Controls.AddRange(new Control[] { picProductImage, btnChooseImage });
        _grpInput.Controls.AddRange(buttons);
    }

    private void ConfigureGrid()
    {
        dgvProducts.AutoGenerateColumns = false;
        dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProducts.ReadOnly = true;
        dgvProducts.MultiSelect = false;
        dgvProducts.AllowUserToAddRows = false;
        dgvProducts.AllowUserToDeleteRows = false;
        dgvProducts.RowHeadersVisible = false;
        dgvProducts.Columns.Clear();

        DataGridViewTextBoxColumn Col(string prop, string header, float weight, bool right = false, bool money = false)
        {
            var c = new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, FillWeight = weight };
            if (right) c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            if (money)
            {
                c.DefaultCellStyle.Format = "N0";
                c.DefaultCellStyle.FormatProvider = _vi;
            }
            return c;
        }

        dgvProducts.Columns.AddRange(
            Col(nameof(Product.Id), "Mã SP", 9),
            Col(nameof(Product.Name), "Tên sản phẩm", 25),
            Col(nameof(Product.Category), "Loại", 14),
            Col(nameof(Product.Price), "Đơn giá (VNĐ)", 15, true, true),
            Col(nameof(Product.Quantity), "Tồn kho", 9, true),
            Col(nameof(Product.TotalValue), "Thành tiền (VNĐ)", 17, true, true),
            Col(nameof(Product.ImageFileName), "Tên file ảnh", 11));

        dgvProducts.SelectionChanged += (s, e) =>
        {
            var p = GetSelectedProduct();
            if (p != null) ShowProduct(p);
        };
    }

    private Product? GetSelectedProduct() =>
        dgvProducts.SelectedRows.Count > 0 ? dgvProducts.SelectedRows[0].DataBoundItem as Product : null;

    private void ShowProduct(Product p)
    {
        errorProvider.Clear();
        txtProductId.Text = p.Id;
        txtProductName.Text = p.Name;
        cboCategory.SelectedItem = p.Category;
        txtPrice.Text = p.Price.ToString("N0", _vi);
        numQuantity.Value = Math.Clamp(p.Quantity, (int)numQuantity.Minimum, (int)numQuantity.Maximum);
        _imagePath = p.ImagePath;
        txtImagePath.Text = p.ImagePath;
        SetPicture(p.ImagePath);
    }

    private void UpdateTotals()
    {
        lblTotals.Text = string.Format(_vi,
            "Số sản phẩm: {0}   |   Tổng số lượng: {1:N0}   |   Tổng giá trị kho: {2:N0} VNĐ",
            _products.Count, _products.Sum(p => p.Quantity), _products.Sum(p => p.TotalValue));
    }

    // ===================== ẢNH =====================
    private void ChooseImage()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Chọn ảnh sản phẩm",
            Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            CheckFileExists = true
        };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;

        _imagePath = ofd.FileName;
        txtImagePath.Text = _imagePath;
        SetPicture(_imagePath);
    }

    private void SetPicture(string path)
    {
        var old = picProductImage.Image;
        picProductImage.Image = null;
        old?.Dispose();

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var img = Image.FromStream(fs);
            picProductImage.Image = new Bitmap(img);   // copy ra để không khóa file
        }
        catch (Exception ex) when (ex is ArgumentException or OutOfMemoryException or IOException) { }
    }

    // ===================== VALIDATION =====================
    private bool TryParsePrice(out decimal price) =>
        decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number, _vi, out price);

    private bool ValidateInput()
    {
        errorProvider.Clear();
        bool ok = true;

        if (string.IsNullOrWhiteSpace(txtProductName.Text))
        {
            errorProvider.SetError(txtProductName, "Tên sản phẩm không được để trống");
            ok = false;
        }
        if (cboCategory.SelectedIndex == -1)
        {
            errorProvider.SetError(cboCategory, "Vui lòng chọn loại sản phẩm");
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

    // ===================== THÊM / SỬA / XÓA =====================
    private string NextId()
    {
        int max = 0;
        foreach (var p in _products)
            if (p.Id.StartsWith("SP") && int.TryParse(p.Id.AsSpan(2), out int n))
                max = Math.Max(max, n);
        return $"SP{max + 1:000}";
    }

    private void AddProduct()
    {
        if (!ValidateInput()) return;
        TryParsePrice(out decimal price);

        _products.Add(new Product
        {
            Id = NextId(),
            Name = txtProductName.Text.Trim(),
            Category = cboCategory.SelectedItem!.ToString()!,
            Price = price,
            Quantity = (int)numQuantity.Value,
            ImagePath = _imagePath
        });
        ClearForm();
    }

    private void UpdateProduct()
    {
        var p = GetSelectedProduct();
        if (p is null)
        {
            MessageBox.Show(this, "Vui lòng chọn một sản phẩm trên danh sách.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!ValidateInput()) return;

        TryParsePrice(out decimal price);
        p.Name = txtProductName.Text.Trim();
        p.Category = cboCategory.SelectedItem!.ToString()!;
        p.Price = price;
        p.Quantity = (int)numQuantity.Value;
        p.ImagePath = _imagePath;
        _bs.ResetItem(_products.IndexOf(p));
    }

    private void DeleteProduct()
    {
        var p = GetSelectedProduct();
        if (p is null)
        {
            MessageBox.Show(this, "Vui lòng chọn một sản phẩm để xóa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(this, $"Bạn có chắc muốn xóa sản phẩm \"{p.Name}\" không?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;

        _products.Remove(p);
        if (_products.Count == 0) ClearForm();
    }

    private void ClearForm()
    {
        errorProvider.Clear();
        txtProductId.Text = NextId();
        txtProductName.Clear();
        cboCategory.SelectedIndex = -1;
        txtPrice.Clear();
        numQuantity.Value = 0;
        _imagePath = "";
        txtImagePath.Clear();
        SetPicture("");
        dgvProducts.ClearSelection();
        txtProductName.Focus();
    }

    // ===================== CSV =====================
    private const char Sep = ';';

    private static string Csv(string s)
    {
        s = s.Replace("\r", " ").Replace("\n", " ");
        return s.IndexOfAny(new[] { Sep, '"' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }

    private void ExportCsv()
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Xuất báo cáo CSV",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = "csv",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = $"san-pham-{DateTime.Now:yyyyMMdd}.csv"
        };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(Sep, "Id", "Name", "Category", "Price", "Quantity", "ImagePath"));
        foreach (var p in _products)
            sb.AppendLine(string.Join(Sep, Csv(p.Id), Csv(p.Name), Csv(p.Category),
                p.Price.ToString(CultureInfo.InvariantCulture),
                p.Quantity.ToString(CultureInfo.InvariantCulture), Csv(p.ImagePath)));

        try
        {
            File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show(this, $"Đã xuất {_products.Count} sản phẩm.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Không ghi được file.\n{ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportCsv()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Chọn file CSV để nhập",
            Filter = "CSV (*.csv)|*.csv|Tất cả file (*.*)|*.*",
            CheckFileExists = true
        };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;

        string[] lines;
        try { lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Không đọc được file.\n{ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (lines.Length < 2) return;

        char sep = lines[0].Contains(';') ? ';' : ',';
        var list = new List<Product>();
        int bad = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var f = SplitCsvLine(lines[i], sep);
            if (f.Count < 5 || string.IsNullOrWhiteSpace(f[1])
                || !decimal.TryParse(f[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) || price < 0
                || !int.TryParse(f[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int qty) || qty < 0)
            {
                bad++;
                continue;
            }
            list.Add(new Product
            {
                Id = f[0].Trim(), Name = f[1].Trim(), Category = f[2].Trim(),
                Price = price, Quantity = qty, ImagePath = f.Count > 5 ? f[5].Trim() : ""
            });
        }

        _products.RaiseListChangedEvents = false;
        try
        {
            _products.Clear();
            foreach (var p in list) _products.Add(p);
            var seen = new HashSet<string>();
            foreach (var p in _products)
                if (string.IsNullOrWhiteSpace(p.Id) || !seen.Add(p.Id)) { p.Id = NextId(); seen.Add(p.Id); }
        }
        finally { _products.RaiseListChangedEvents = true; }
        _products.ResetBindings();

        foreach (var cat in _products.Select(p => p.Category).Where(c => c.Length > 0).Distinct())
            if (!cboCategory.Items.Contains(cat)) cboCategory.Items.Add(cat);

        ClearForm();
        MessageBox.Show(this, $"Đã nhập {list.Count} sản phẩm." + (bad > 0 ? $"\nBỏ qua {bad} dòng sai định dạng." : ""),
            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static List<string> SplitCsvLine(string line, char sep)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == sep) { result.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(c);
        }
        result.Add(sb.ToString());
        return result;
    }
}
