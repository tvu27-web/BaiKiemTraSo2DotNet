using System.ComponentModel;
using System.Drawing.Text;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Bai2QuanLyKho;   // đổi cho khớp namespace trong Program.cs của bạn

// Form viết hoàn toàn bằng code: KHÔNG có file Designer, KHÔNG dùng partial
public class frmInventoryManager : Form
{
    // ===== Dữ liệu =====
    private readonly BindingList<Product> _products = new();
    private readonly BindingSource _bs = new();
    private readonly CultureInfo _vi = new("vi-VN");
    private string _imagePath = "";   // đường dẫn ảnh đang chọn trên form

    // ===== Thanh điều khiển =====
    private readonly MenuStrip _menu = new();
    private readonly ToolStrip _tool = new();
    private readonly StatusStrip _status = new();
    private readonly ToolStripStatusLabel _lblCount = new();
    private readonly ToolStripStatusLabel _lblQty = new();
    private readonly ToolStripStatusLabel _lblValue = new();
    private readonly ToolStripStatusLabel _lblClock = new();
    private readonly System.Windows.Forms.Timer _clockTimer = new();

    // ===== Khu vực nhập liệu =====
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
    private readonly ErrorProvider errorProvider = new();

    // ===== Khu vực hiển thị =====
    private readonly DataGridView dgvProducts = new();

    public frmInventoryManager()
    {
        InitializeUI();
        ConfigureGrid();
        SeedData();
        BindData();
        StartClock();
        UpdateStatus();
        ClearForm();
    }

    // =====================================================================
    // 1. DỰNG GIAO DIỆN BẰNG CODE
    // =====================================================================
    private void InitializeUI()
    {
        Text = "Quản lý kho hàng";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1050, 650);
        MinimumSize = new Size(1000, 600);

        BuildInputGroup();
        BuildMenuAndToolbar();
        BuildStatusBar();

        dgvProducts.Dock = DockStyle.Fill;

        // THỨ TỰ ADD RẤT QUAN TRỌNG khi dùng Dock:
        // control Fill add TRƯỚC, các control Top/Bottom add SAU,
        // control add sau cùng sẽ nằm sát mép trên cùng (menu).
        Controls.Add(dgvProducts);
        Controls.Add(_status);
        Controls.Add(_grpInput);
        Controls.Add(_tool);
        Controls.Add(_menu);
        MainMenuStrip = _menu;

        errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
    }

    // Thêm "nhãn + control" vào group nhập liệu
    private void AddField(string caption, Control ctl, int labelX, int y, int width = 200)
    {
        var lbl = new Label { Text = caption, Left = labelX, Top = y + 4, AutoSize = true };
        ctl.Left = labelX + 105;
        ctl.Top = y;
        ctl.Width = width;
        _grpInput.Controls.Add(lbl);
        _grpInput.Controls.Add(ctl);
    }

    private static Button MakeButton(string text, int left, int top, int width)
    {
        return new Button { Text = text, Left = left, Top = top, Width = width, Height = 32 };
    }

    private void BuildInputGroup()
    {
        _grpInput.Text = "Thông tin sản phẩm";
        _grpInput.Dock = DockStyle.Top;
        _grpInput.Height = 220;

        // --- Cột trái ---
        txtProductId.ReadOnly = true;
        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCategory.Items.AddRange(new object[]
        {
            "Điện thoại", "Laptop", "Máy tính bảng", "Phụ kiện", "Màn hình"
        });
        txtPrice.TextAlign = HorizontalAlignment.Right;

        AddField("Mã sản phẩm:", txtProductId, 15, 30);
        AddField("Tên sản phẩm:", txtProductName, 15, 65);
        AddField("Loại sản phẩm:", cboCategory, 15, 100);
        AddField("Đơn giá (VNĐ):", txtPrice, 15, 135);

        // --- Cột phải ---
        numQuantity.Minimum = 0;
        numQuantity.Maximum = 1_000_000;
        txtImagePath.ReadOnly = true;

        AddField("Số lượng tồn:", numQuantity, 340, 30, 220);
        AddField("Đường dẫn ảnh:", txtImagePath, 340, 65, 220);

        // --- Ảnh đại diện (neo bên phải) ---
        picProductImage.Left = 830;
        picProductImage.Top = 25;
        picProductImage.Size = new Size(190, 140);
        picProductImage.BorderStyle = BorderStyle.FixedSingle;
        picProductImage.SizeMode = PictureBoxSizeMode.Zoom;
        picProductImage.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnChooseImage.Text = "Chọn ảnh";
        btnChooseImage.Left = 830;
        btnChooseImage.Top = 172;
        btnChooseImage.Size = new Size(190, 32);
        btnChooseImage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnChooseImage.Click += (s, e) => ChooseImage();

        // --- Nút chức năng ---
        btnAdd.Text = "Thêm mới";
        btnUpdate.Text = "Cập nhật";
        btnDelete.Text = "Xóa";
        btnClear.Text = "Làm mới";
        int x = 120;
        foreach (var b in new[] { btnAdd, btnUpdate, btnDelete, btnClear })
        {
            b.SetBounds(x, 172, 100, 32);
            x += 110;
        }
        btnAdd.Click += (s, e) => AddProduct();
        btnUpdate.Click += (s, e) => UpdateProduct();
        btnDelete.Click += (s, e) => DeleteProduct();
        btnClear.Click += (s, e) => ClearForm();

        _grpInput.Controls.AddRange(new Control[]
        {
            picProductImage, btnChooseImage, btnAdd, btnUpdate, btnDelete, btnClear
        });
    }

    private void BuildMenuAndToolbar()
    {
        // Icon vẽ bằng code từ font Segoe MDL2 Assets (có sẵn trên Windows 10/11)
        Image icoOpen = GlyphIcon("\uE8E5");
        Image icoSave = GlyphIcon("\uE74E");
        Image icoExit = GlyphIcon("\uE7E8");
        Image icoImport = GlyphIcon("\uE896");
        Image icoExport = GlyphIcon("\uE898");

        // --- MenuStrip ---
        var mnuSystem = new ToolStripMenuItem("&System");
        mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Open File...", icoOpen, (s, e) => OpenJsonFile())
        { ShortcutKeys = Keys.Control | Keys.O });
        mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Save Report...", icoSave, (s, e) => SaveReport())
        { ShortcutKeys = Keys.Control | Keys.S });
        mnuSystem.DropDownItems.Add(new ToolStripSeparator());
        // Close() sẽ kích hoạt OnFormClosing -> hộp thoại Yes/No hiện ở đó (chỉ 1 chỗ)
        mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Exit", icoExit, (s, e) => Close())
        { ShortcutKeys = Keys.Alt | Keys.F4 });

        var mnuTools = new ToolStripMenuItem("&Tools");
        mnuTools.DropDownItems.Add(new ToolStripMenuItem("Import CSV...", icoImport, (s, e) => ImportCsv()));
        mnuTools.DropDownItems.Add(new ToolStripMenuItem("Export CSV...", icoExport, (s, e) => ExportCsv()));

        _menu.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuTools });

        // --- ToolStrip ---
        _tool.ImageScalingSize = new Size(24, 24);
        _tool.Items.Add(MakeToolButton("Open File", icoOpen, (s, e) => OpenJsonFile()));
        _tool.Items.Add(MakeToolButton("Save Report", icoSave, (s, e) => SaveReport()));
        _tool.Items.Add(new ToolStripSeparator());
        _tool.Items.Add(MakeToolButton("Import CSV", icoImport, (s, e) => ImportCsv()));
        _tool.Items.Add(MakeToolButton("Export CSV", icoExport, (s, e) => ExportCsv()));
        _tool.Items.Add(new ToolStripSeparator());
        _tool.Items.Add(MakeToolButton("Exit", icoExit, (s, e) => Close()));
    }

    private static ToolStripButton MakeToolButton(string text, Image icon, EventHandler onClick)
    {
        return new ToolStripButton(text, icon, onClick)
        {
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
            TextImageRelation = TextImageRelation.ImageAboveText
        };
    }

    private static Image GlyphIcon(string glyph, int size = 24)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        using var font = new Font("Segoe MDL2 Assets", size * 0.75f, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(Color.FromArgb(40, 40, 40));
        using var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(glyph, font, brush, new RectangleF(0, 0, size, size), sf);
        return bmp;
    }

    private void BuildStatusBar()
    {
        var spring = new ToolStripStatusLabel { Spring = true };
        _status.Items.AddRange(new ToolStripItem[]
        {
            _lblCount, new ToolStripSeparator(),
            _lblQty, new ToolStripSeparator(),
            _lblValue, spring, _lblClock
        });
    }

    private void StartClock()
    {
        _clockTimer.Interval = 1000;
        _clockTimer.Tick += (s, e) => _lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        _clockTimer.Start();
        _lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    // =====================================================================
    // 2. DATAGRIDVIEW + BINDING
    // =====================================================================
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

        DataGridViewTextBoxColumn Col(string prop, string header, float weight, bool money = false)
        {
            var c = new DataGridViewTextBoxColumn
            {
                DataPropertyName = prop,
                HeaderText = header,
                FillWeight = weight
            };
            if (money)
            {
                c.DefaultCellStyle.Format = "N0";
                c.DefaultCellStyle.FormatProvider = _vi;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            return c;
        }

        var colQty = Col(nameof(Product.Quantity), "Tồn kho", 9);
        colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        dgvProducts.Columns.AddRange(
            Col(nameof(Product.Id), "Mã SP", 9),
            Col(nameof(Product.Name), "Tên sản phẩm", 25),
            Col(nameof(Product.Category), "Loại", 14),
            Col(nameof(Product.Price), "Đơn giá (VNĐ)", 15, money: true),
            colQty,
            Col(nameof(Product.TotalValue), "Thành tiền (VNĐ)", 17, money: true),
            Col(nameof(Product.ImageFileName), "Tên file ảnh", 11));

        // Click dòng -> đổ thông tin lên form (xem giải thích cuối bài)
        dgvProducts.SelectionChanged += (s, e) =>
        {
            var p = GetSelectedProduct();
            if (p != null) ShowProduct(p);
        };
    }

    private void SeedData()
    {
        _products.Add(new Product { Id = "SP001", Name = "iPhone 15", Category = "Điện thoại", Price = 22000000, Quantity = 10 });
        _products.Add(new Product { Id = "SP002", Name = "Dell XPS 13", Category = "Laptop", Price = 35000000, Quantity = 5 });
        _products.Add(new Product { Id = "SP003", Name = "Tai nghe AirPods", Category = "Phụ kiện", Price = 4500000, Quantity = 30 });
    }

    private void BindData()
    {
        _bs.DataSource = _products;
        dgvProducts.DataSource = _bs;

        // Mọi thay đổi danh sách (Thêm/Sửa/Xóa/Import) đều đi qua đây
        _bs.ListChanged += (s, e) => UpdateStatus();
    }

    private Product? GetSelectedProduct()
    {
        return dgvProducts.SelectedRows.Count > 0
            ? dgvProducts.SelectedRows[0].DataBoundItem as Product
            : null;
    }

    private void ShowProduct(Product p)
    {
        errorProvider.Clear();
        txtProductId.Text = p.Id;
        txtProductName.Text = p.Name;
        cboCategory.SelectedItem = p.Category;      // không có trong danh sách -> tự về -1
        txtPrice.Text = p.Price.ToString("N0", _vi);
        numQuantity.Value = Math.Clamp(p.Quantity, (int)numQuantity.Minimum, (int)numQuantity.Maximum);

        _imagePath = p.ImagePath;
        txtImagePath.Text = p.ImagePath;
        SetPicture(p.ImagePath);
    }

    // =====================================================================
    // 3. XỬ LÝ HÌNH ẢNH (OpenFileDialog)
    // =====================================================================
    private void ChooseImage()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Chọn ảnh sản phẩm",
            Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            CheckFileExists = true,
            Multiselect = false
        };

        if (ofd.ShowDialog(this) != DialogResult.OK) return;

        _imagePath = ofd.FileName;
        txtImagePath.Text = _imagePath;
        SetPicture(_imagePath);
    }

    // Đọc ảnh qua stream rồi copy ra Bitmap để KHÔNG bị khóa file ảnh trên đĩa
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
            picProductImage.Image = new Bitmap(img);
        }
        catch (Exception ex) when (ex is ArgumentException or OutOfMemoryException or IOException)
        {
            // File hỏng hoặc không phải ảnh thật: bỏ qua, để PictureBox trống
        }
    }

    // =====================================================================
    // 4. VALIDATION
    // =====================================================================
    private bool TryParsePrice(out decimal price)
    {
        return decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Number, _vi, out price);
    }

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

    // =====================================================================
    // 5. THÊM / SỬA / XÓA / LÀM MỚI
    // =====================================================================
    private string NextId()
    {
        int max = 0;
        foreach (var p in _products)
        {
            if (p.Id.StartsWith("SP") && int.TryParse(p.Id.AsSpan(2), out int n))
                max = Math.Max(max, n);
        }
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
            Info("Vui lòng chọn một sản phẩm trên danh sách.");
            return;
        }

        if (!ValidateInput()) return;

        TryParsePrice(out decimal price);
        p.Name = txtProductName.Text.Trim();
        p.Category = cboCategory.SelectedItem!.ToString()!;
        p.Price = price;
        p.Quantity = (int)numQuantity.Value;
        p.ImagePath = _imagePath;

        _bs.ResetItem(_products.IndexOf(p));   // báo Grid vẽ lại dòng + cập nhật StatusStrip
    }

    private void DeleteProduct()
    {
        var p = GetSelectedProduct();
        if (p is null)
        {
            Info("Vui lòng chọn một sản phẩm để xóa.");
            return;
        }

        var result = MessageBox.Show(this,
            $"Bạn có chắc muốn xóa sản phẩm \"{p.Name}\" không?",
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

    // =====================================================================
    // 6. STATUSSTRIP
    // =====================================================================
    private void UpdateStatus()
    {
        int totalQty = _products.Sum(p => p.Quantity);
        decimal totalValue = _products.Sum(p => p.TotalValue);

        _lblCount.Text = $"Số sản phẩm: {_products.Count}";
        _lblQty.Text = $"Tổng số lượng: {totalQty.ToString("N0", _vi)}";
        _lblValue.Text = $"Tổng giá trị kho: {totalValue.ToString("N0", _vi)} VNĐ";
    }

    // =====================================================================
    // 7. XUẤT / NHẬP CSV (SaveFileDialog & OpenFileDialog)
    // =====================================================================
    private const char Sep = ';';   // dấu chấm phẩy: Excel tiếng Việt mở ra đúng cột

    private static string Csv(string s)
    {
        s = s.Replace("\r", " ").Replace("\n", " ");
        return s.IndexOfAny(new[] { Sep, '"' }) >= 0
            ? "\"" + s.Replace("\"", "\"\"") + "\""
            : s;
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
            FileName = $"kho-hang-{DateTime.Now:yyyyMMdd}.csv"
        };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(Sep, "Id", "Name", "Category", "Price", "Quantity", "ImagePath"));
        foreach (var p in _products)
        {
            sb.AppendLine(string.Join(Sep,
                Csv(p.Id), Csv(p.Name), Csv(p.Category),
                p.Price.ToString(CultureInfo.InvariantCulture),
                p.Quantity.ToString(CultureInfo.InvariantCulture),
                Csv(p.ImagePath)));
        }

        try
        {
            // UTF-8 có BOM để Excel đọc đúng tiếng Việt
            File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
            Info($"Đã xuất {_products.Count} sản phẩm ra file:\n{sfd.FileName}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Error($"Không ghi được file (có thể file đang mở trong Excel).\n{ex.Message}");
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

        if (_products.Count > 0)
        {
            var confirm = MessageBox.Show(this,
                "Dữ liệu hiện tại sẽ bị thay thế bằng dữ liệu trong file. Tiếp tục?",
                "Xác nhận nhập", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
        }

        string[] lines;
        try { lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Error($"Không đọc được file.\n{ex.Message}");
            return;
        }

        if (lines.Length < 2)
        {
            Info("File không có dữ liệu (cần ít nhất 1 dòng tiêu đề và 1 dòng dữ liệu).");
            return;
        }

        char sep = lines[0].Contains(';') ? ';' : ',';   // tự nhận dạng dấu phân cách
        var list = new List<Product>();
        int bad = 0;

        for (int i = 1; i < lines.Length; i++)          // i = 0 là dòng tiêu đề
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var f = SplitCsvLine(lines[i], sep);
            if (f.Count < 5
                || string.IsNullOrWhiteSpace(f[1])
                || !decimal.TryParse(f[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price)
                || price < 0
                || !int.TryParse(f[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int qty)
                || qty < 0)
            {
                bad++;
                continue;
            }

            list.Add(new Product
            {
                Id = f[0].Trim(),
                Name = f[1].Trim(),
                Category = f[2].Trim(),
                Price = price,
                Quantity = qty,
                ImagePath = f.Count > 5 ? f[5].Trim() : ""
            });
        }

        ReplaceAll(list);
        Info($"Đã nhập {list.Count} sản phẩm." + (bad > 0 ? $"\nBỏ qua {bad} dòng sai định dạng." : ""));
    }

    // Tách 1 dòng CSV, hỗ trợ trường nằm trong dấu ngoặc kép
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

    // Nạp lại toàn bộ danh sách (dùng chung cho Import CSV và Open File)
    private void ReplaceAll(List<Product> items)
    {
        _products.RaiseListChangedEvents = false;   // tắt thông báo để nạp nhanh, tránh Grid vẽ lại từng dòng
        try
        {
            _products.Clear();
            foreach (var p in items) _products.Add(p);

            // Mã rỗng hoặc trùng thì cấp mã mới
            var seen = new HashSet<string>();
            foreach (var p in _products)
            {
                if (string.IsNullOrWhiteSpace(p.Id) || !seen.Add(p.Id))
                {
                    p.Id = NextId();
                    seen.Add(p.Id);
                }
            }
        }
        finally
        {
            _products.RaiseListChangedEvents = true;
        }
        _products.ResetBindings();   // báo Grid + StatusStrip cập nhật 1 lần

        // Thêm các loại mới (từ file) vào ComboBox nếu chưa có
        foreach (var cat in _products.Select(p => p.Category).Where(c => c.Length > 0).Distinct())
            if (!cboCategory.Items.Contains(cat)) cboCategory.Items.Add(cat);

        ClearForm();
    }

    // =====================================================================
    // 8. OPEN FILE (JSON) & SAVE REPORT (TXT / JSON)
    // =====================================================================
    private void OpenJsonFile()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Mở file dữ liệu kho",
            Filter = "Dữ liệu JSON (*.json)|*.json",
            CheckFileExists = true
        };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            string json = File.ReadAllText(ofd.FileName, Encoding.UTF8);
            var list = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
            list.RemoveAll(p => string.IsNullOrWhiteSpace(p.Name));
            ReplaceAll(list);
            Info($"Đã mở {list.Count} sản phẩm từ file JSON.");
        }
        catch (JsonException)
        {
            Error("File JSON không đúng định dạng danh sách sản phẩm.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Error($"Không đọc được file.\n{ex.Message}");
        }
    }

    private void SaveReport()
    {
        using var sfd = new SaveFileDialog
        {
            Title = "Lưu báo cáo",
            Filter = "Báo cáo văn bản (*.txt)|*.txt|Dữ liệu JSON (*.json)|*.json",
            DefaultExt = "txt",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = $"bao-cao-kho-{DateTime.Now:yyyyMMdd}"
        };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            if (Path.GetExtension(sfd.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                var opt = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(sfd.FileName, JsonSerializer.Serialize(_products.ToList(), opt), Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(sfd.FileName, BuildTextReport(), Encoding.UTF8);
            }
            Info($"Đã lưu báo cáo:\n{sfd.FileName}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Error($"Không ghi được file.\n{ex.Message}");
        }
    }

    private string BuildTextReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("BÁO CÁO TỔNG HỢP KHO HÀNG");
        sb.AppendLine($"Thời gian lập: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine(new string('=', 90));
        sb.AppendLine(string.Format(_vi, "{0,-8}{1,-28}{2,-16}{3,14}{4,8}{5,16}",
            "Mã", "Tên sản phẩm", "Loại", "Đơn giá", "SL", "Thành tiền"));
        sb.AppendLine(new string('-', 90));
        foreach (var p in _products)
        {
            sb.AppendLine(string.Format(_vi, "{0,-8}{1,-28}{2,-16}{3,14:N0}{4,8}{5,16:N0}",
                p.Id, p.Name, p.Category, p.Price, p.Quantity, p.TotalValue));
        }
        sb.AppendLine(new string('=', 90));
        sb.AppendLine($"Số sản phẩm      : {_products.Count}");
        sb.AppendLine($"Tổng số lượng    : {_products.Sum(p => p.Quantity).ToString("N0", _vi)}");
        sb.AppendLine($"Tổng giá trị kho : {_products.Sum(p => p.TotalValue).ToString("N0", _vi)} VNĐ");
        return sb.ToString();
    }

    // =====================================================================
    // 9. THOÁT CHƯƠNG TRÌNH (MessageBox Yes/No)
    // =====================================================================
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Chạy cho cả: bấm X, menu Exit, nút Exit trên ToolStrip, Alt+F4
        if (e.CloseReason == CloseReason.UserClosing)
        {
            var result = MessageBox.Show(this,
                "Bạn có chắc chắn muốn thoát chương trình không?",
                "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) e.Cancel = true;
        }
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _clockTimer.Stop();
        base.OnFormClosed(e);
    }

    // ===== Tiện ích hiển thị thông báo =====
    private void Info(string message) =>
        MessageBox.Show(this, message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void Error(string message) =>
        MessageBox.Show(this, message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
