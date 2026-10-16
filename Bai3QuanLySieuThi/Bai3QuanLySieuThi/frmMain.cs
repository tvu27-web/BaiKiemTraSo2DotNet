namespace Bai3QuanLySieuThi;

// Form cha MDI: viết hoàn toàn bằng code
public class frmMain : Form
{
    private readonly MenuStrip _menu = new();
    private readonly StatusStrip _status = new();
    private readonly ToolStripStatusLabel _lblUser = new();
    private readonly ToolStripStatusLabel _lblSync = new();
    private readonly ToolStripStatusLabel _lblBusy = new();
    private readonly ToolStripProgressBar _progress = new();

    private int _busyCount;          // số tác vụ nặng đang chạy (dùng chung 1 ProgressBar)
    private bool _skipConfirm;       // true khi Đăng xuất (không hỏi lại lúc đóng form)

    public bool LoggedOut { get; private set; }

    public frmMain(string userName)
    {
        Text = "Hệ thống quản lý siêu thị";
        IsMdiContainer = true;                     // BẮT BUỘC: biến form thành MDI cha
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(900, 600);

        BuildMenu();
        BuildStatusBar(userName);

        Controls.Add(_status);
        Controls.Add(_menu);
        MainMenuStrip = _menu;
    }

    // =====================================================================
    // GIAO DIỆN
    // =====================================================================
    private void BuildMenu()
    {
        // --- Hệ thống ---
        var mnuSystem = new ToolStripMenuItem("&Hệ thống");
        mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Đăng xuất", null, (s, e) => Logout()));
        mnuSystem.DropDownItems.Add(new ToolStripSeparator());
        mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Thoát", null, (s, e) => Close())
        { ShortcutKeys = Keys.Alt | Keys.F4 });

        // --- Danh mục ---
        var mnuCatalog = new ToolStripMenuItem("&Danh mục");
        mnuCatalog.DropDownItems.Add(new ToolStripMenuItem("Quản lý Sản phẩm", null, (s, e) => OpenChild<frmProductCatalog>()));
        mnuCatalog.DropDownItems.Add(new ToolStripMenuItem("Quản lý Nhập xuất", null, (s, e) => OpenChild<frmStockMovement>()));

        // --- Báo cáo ---
        var mnuReport = new ToolStripMenuItem("&Báo cáo");
        mnuReport.DropDownItems.Add(new ToolStripMenuItem("Thống kê Doanh thu", null, (s, e) => OpenChild<frmAsyncReport>()));

        // --- Cửa sổ ---
        var mnuWindow = new ToolStripMenuItem("&Cửa sổ");
        mnuWindow.DropDownItems.Add(new ToolStripMenuItem("Cascade", null, (s, e) => LayoutMdi(MdiLayout.Cascade)));
        mnuWindow.DropDownItems.Add(new ToolStripMenuItem("Tile Horizontal", null, (s, e) => LayoutMdi(MdiLayout.TileHorizontal)));
        mnuWindow.DropDownItems.Add(new ToolStripMenuItem("Tile Vertical", null, (s, e) => LayoutMdi(MdiLayout.TileVertical)));

        _menu.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuCatalog, mnuReport, mnuWindow });
        _menu.MdiWindowListItem = mnuWindow;   // menu Cửa sổ tự liệt kê các form con đang mở
    }

    private void BuildStatusBar(string userName)
    {
        _lblUser.Text = $"Người dùng: {userName}";
        _lblSync.Text = "Đồng bộ: chưa chạy";
        _lblSync.Spring = true;                    // chiếm phần trống giữa
        _lblSync.TextAlign = ContentAlignment.MiddleLeft;

        _lblBusy.Visible = false;
        _progress.Minimum = 0;
        _progress.Maximum = 100;
        _progress.Width = 200;
        _progress.Visible = false;

        _status.Items.AddRange(new ToolStripItem[]
        {
            _lblUser, new ToolStripSeparator(), _lblSync, _lblBusy, _progress
        });
    }

    // =====================================================================
    // SCENARIO A: ĐIỀU HƯỚNG MDI
    // =====================================================================
    private void OpenChild<T>() where T : Form, new()
    {
        // Đã mở rồi thì chỉ đưa lên trước, không tạo thêm bản thứ 2
        var existing = MdiChildren.OfType<T>().FirstOrDefault();
        if (existing != null)
        {
            if (existing.WindowState == FormWindowState.Minimized)
                existing.WindowState = FormWindowState.Normal;
            existing.Activate();
            return;
        }

        var child = new T { MdiParent = this };    // gán MdiParent TRƯỚC khi Show()
        child.Show();
    }

    // =====================================================================
    // PROGRESSBAR DÙNG CHUNG (form con gọi, luôn gọi từ UI Thread)
    // =====================================================================
    public void BeginBusy(string text)
    {
        _busyCount++;
        _lblBusy.Text = text;
        _lblBusy.Visible = true;
        _progress.Value = 0;
        _progress.Visible = true;
    }

    public void SetProgress(int percent)
    {
        _progress.Value = Math.Clamp(percent, 0, 100);
    }

    public void EndBusy()
    {
        _busyCount = Math.Max(0, _busyCount - 1);
        if (_busyCount == 0)
        {
            _progress.Visible = false;
            _lblBusy.Visible = false;
        }
    }

    // Form con cập nhật dòng thông tin đồng bộ trên StatusStrip (gọi từ UI Thread)
    public void SetSyncText(string text)
    {
        _lblSync.Text = text;
    }

    // =====================================================================
    // ĐĂNG XUẤT / THOÁT
    // =====================================================================
    private void Logout()
    {
        var result = MessageBox.Show(this, "Bạn có chắc muốn đăng xuất?", "Đăng xuất",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;

        LoggedOut = true;
        _skipConfirm = true;
        Close();               // Program.cs sẽ thấy LoggedOut = true và hiện lại form đăng nhập
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_skipConfirm && e.CloseReason == CloseReason.UserClosing)
        {
            var result = MessageBox.Show(this, "Bạn có chắc chắn muốn thoát chương trình không?",
                "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
