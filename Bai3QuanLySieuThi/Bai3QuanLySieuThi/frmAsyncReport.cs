using System.ComponentModel;
using System.Globalization;

namespace Bai3QuanLySieuThi;

public class RevenueRow
{
    public string Month { get; set; } = "";
    public int Orders { get; set; }
    public decimal Revenue { get; set; }
    public double Growth { get; set; }
}

public class SyncLogRow
{
    public string Time { get; set; } = "";
    public decimal Rate { get; set; }
    public int NewOrders { get; set; }
    public int TotalOrders { get; set; }
}

// Form con 2: Scenario B (async/await + Task.Run) và Scenario C (cập nhật UI an toàn từ thread phụ)
public class frmAsyncReport : Form
{
    private readonly CultureInfo _vi = new("vi-VN");

    private readonly BindingList<RevenueRow> _rows = new();
    private readonly BindingList<SyncLogRow> _syncLog = new();

    private CancellationTokenSource? _reportCts;   // hủy tác vụ tải báo cáo
    private CancellationTokenSource? _syncCts;     // dừng tiến trình đồng bộ nền
    private int _clicks;

    // --- Điều khiển ---
    private readonly Button btnLoadReport = new();
    private readonly Button btnCancel = new();
    private readonly Button btnClickTest = new();
    private readonly Label lblClickCount = new();
    private readonly Label lblStatus = new();

    private readonly Label lblRate = new();
    private readonly Label lblNewOrders = new();
    private readonly Label lblLastSync = new();
    private readonly Button btnToggleSync = new();

    private readonly DataGridView dgvReport = new();
    private readonly DataGridView dgvSyncLog = new();

    // Dữ liệu 1 lần đồng bộ, gửi từ thread phụ sang UI thread
    private record SyncSnapshot(DateTime Time, decimal Rate, int NewOrders, int TotalOrders);

    public frmAsyncReport()
    {
        Text = "Thống kê Doanh thu & Đồng bộ dữ liệu";
        ClientSize = new Size(980, 560);

        BuildUI();
        ConfigureReportGrid();
        ConfigureLogGrid();

        dgvReport.DataSource = _rows;
        dgvSyncLog.DataSource = _syncLog;
    }

    // =====================================================================
    // GIAO DIỆN
    // =====================================================================
    private void BuildUI()
    {
        // --- Thanh nút phía trên ---
        btnLoadReport.Text = "Tải Báo Cáo Doanh Thu";
        btnLoadReport.Size = new Size(190, 34);
        btnLoadReport.Click += btnLoadReport_Click;

        btnCancel.Text = "Hủy";
        btnCancel.Size = new Size(80, 34);
        btnCancel.Enabled = false;
        btnCancel.Click += (s, e) => _reportCts?.Cancel();

        btnClickTest.Text = "Bấm thử (kiểm tra UI)";
        btnClickTest.Size = new Size(170, 34);
        btnClickTest.Click += (s, e) => lblClickCount.Text = $"Số lần bấm: {++_clicks}";

        lblClickCount.Text = "Số lần bấm: 0";
        lblClickCount.AutoSize = true;
        lblClickCount.Margin = new Padding(10, 10, 0, 0);

        lblStatus.Text = "Sẵn sàng";
        lblStatus.AutoSize = true;
        lblStatus.Margin = new Padding(20, 10, 0, 0);

        var pnlTop = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(8) };
        pnlTop.Controls.AddRange(new Control[]
        {
            btnLoadReport, btnCancel, btnClickTest, lblClickCount, lblStatus
        });

        // --- Nhóm đồng bộ thời gian thực ---
        lblRate.Text = "Tỷ giá USD/VND: --";
        lblNewOrders.Text = "Đơn hàng mới: --";
        lblLastSync.Text = "Cập nhật lúc: --";
        foreach (var l in new[] { lblRate, lblNewOrders, lblLastSync })
        {
            l.AutoSize = true;
            l.Margin = new Padding(0, 9, 30, 0);
        }

        btnToggleSync.Text = "Dừng đồng bộ";
        btnToggleSync.Size = new Size(150, 30);
        btnToggleSync.Click += (s, e) =>
        {
            if (_syncCts == null) StartSync(); else StopSync();
        };

        var pnlSync = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 8, 4) };
        pnlSync.Controls.AddRange(new Control[] { lblRate, lblNewOrders, lblLastSync, btnToggleSync });

        var grpSync = new GroupBox
        {
            Text = "Đồng bộ thời gian thực (cập nhật mỗi 2 giây, chạy ở thread phụ)",
            Dock = DockStyle.Top,
            Height = 72
        };
        grpSync.Controls.Add(pnlSync);

        // --- Hai bảng dữ liệu ---
        var grpReport = new GroupBox { Text = "Báo cáo doanh thu theo tháng", Dock = DockStyle.Fill };
        dgvReport.Dock = DockStyle.Fill;
        grpReport.Controls.Add(dgvReport);

        var grpLog = new GroupBox { Text = "Nhật ký đồng bộ", Dock = DockStyle.Fill };
        dgvSyncLog.Dock = DockStyle.Fill;
        grpLog.Controls.Add(dgvSyncLog);

        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tbl.Controls.Add(grpReport, 0, 0);
        tbl.Controls.Add(grpLog, 1, 0);

        // Fill add trước, Top add sau (control add cuối nằm trên cùng)
        Controls.Add(tbl);
        Controls.Add(grpSync);
        Controls.Add(pnlTop);
    }

    private void SetupGrid(DataGridView g)
    {
        g.AutoGenerateColumns = false;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.ReadOnly = true;
        g.MultiSelect = false;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.RowHeadersVisible = false;
        g.Columns.Clear();
    }

    private DataGridViewTextBoxColumn Col(string prop, string header, float weight, string? format = null)
    {
        var c = new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, FillWeight = weight };
        if (format != null)
        {
            c.DefaultCellStyle.Format = format;
            c.DefaultCellStyle.FormatProvider = _vi;
            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        return c;
    }

    private void ConfigureReportGrid()
    {
        SetupGrid(dgvReport);
        dgvReport.Columns.AddRange(
            Col(nameof(RevenueRow.Month), "Tháng", 28),
            Col(nameof(RevenueRow.Orders), "Số đơn hàng", 20, "N0"),
            Col(nameof(RevenueRow.Revenue), "Doanh thu (VNĐ)", 32, "N0"),
            Col(nameof(RevenueRow.Growth), "So tháng trước (%)", 20, "N1"));
    }

    private void ConfigureLogGrid()
    {
        SetupGrid(dgvSyncLog);
        dgvSyncLog.Columns.AddRange(
            Col(nameof(SyncLogRow.Time), "Thời gian", 25),
            Col(nameof(SyncLogRow.Rate), "Tỷ giá", 25, "N0"),
            Col(nameof(SyncLogRow.NewOrders), "Đơn mới", 25, "N0"),
            Col(nameof(SyncLogRow.TotalOrders), "Tổng đơn", 25, "N0"));
    }

    // =====================================================================
    // SCENARIO B: TẢI BÁO CÁO BẤT ĐỒNG BỘ (async / await + Task.Run)
    // =====================================================================
    private async void btnLoadReport_Click(object? sender, EventArgs e)
    {
        var parent = MdiParent as frmMain;

        btnLoadReport.Enabled = false;
        btnCancel.Enabled = true;
        lblStatus.Text = "Đang tải báo cáo...";
        parent?.BeginBusy("Đang tải báo cáo...");      // bật ProgressBar trên StatusStrip của Form cha

        _reportCts = new CancellationTokenSource();
        var token = _reportCts.Token;

        // Progress<T> tạo ở UI Thread nên callback luôn được chuyển về UI Thread an toàn
        var progress = new Progress<int>(percent =>
        {
            if (IsDisposed) return;
            parent?.SetProgress(percent);
            lblStatus.Text = $"Đang xử lý... {percent}%";
        });

        try
        {
            // Phần nặng chạy ở THREAD PHỤ, UI Thread được giải phóng trong lúc await
            List<RevenueRow> result = await Task.Run(() => BuildReport(progress, token), token);

            if (IsDisposed) return;   // form đã đóng trong lúc chờ

            _rows.Clear();
            foreach (var r in result) _rows.Add(r);
            lblStatus.Text = $"Hoàn tất. Tổng doanh thu: {result.Sum(r => r.Revenue).ToString("N0", _vi)} VNĐ";
        }
        catch (OperationCanceledException)
        {
            if (!IsDisposed) lblStatus.Text = "Đã hủy tải báo cáo.";
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                lblStatus.Text = "Lỗi khi tải báo cáo.";
                MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            if (parent is { IsDisposed: false }) parent.EndBusy();
            if (!IsDisposed)
            {
                btnLoadReport.Enabled = true;
                btnCancel.Enabled = false;
            }
        }
    }

    // Chạy ở thread phụ: TUYỆT ĐỐI không đụng tới control nào ở đây
    private static List<RevenueRow> BuildReport(IProgress<int> progress, CancellationToken token)
    {
        var rows = new List<RevenueRow>();
        decimal previous = 0;
        int year = DateTime.Now.Year;

        for (int month = 1; month <= 12; month++)
        {
            token.ThrowIfCancellationRequested();

            Thread.Sleep(350);                     // giả lập truy vấn CSDL chậm

            double acc = 0;                        // giả lập tính toán khối lượng lớn
            for (int i = 1; i < 3_000_000; i++) acc += Math.Sqrt(i) % 7;

            int orders = Random.Shared.Next(800, 1500);
            decimal revenue = orders * Random.Shared.Next(150, 400) * 1000m + (decimal)(acc % 1000);
            double growth = previous == 0 ? 0 : (double)((revenue - previous) / previous * 100m);
            previous = revenue;

            rows.Add(new RevenueRow
            {
                Month = $"Tháng {month:00}/{year}",
                Orders = orders,
                Revenue = revenue,
                Growth = growth
            });

            progress.Report(month * 100 / 12);     // báo tiến độ về UI (an toàn qua Progress<T>)
        }
        return rows;
    }

    // =====================================================================
    // SCENARIO C: ĐỒNG BỘ NỀN + CẬP NHẬT UI AN TOÀN (Invoke)
    // =====================================================================
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        StartSync();
    }

    private void StartSync()
    {
        if (_syncCts != null) return;

        _syncCts = new CancellationTokenSource();
        var token = _syncCts.Token;
        btnToggleSync.Text = "Dừng đồng bộ";

        _ = Task.Run(async () =>
        {
            // ===== TOÀN BỘ KHỐI NÀY CHẠY Ở THREAD PHỤ =====
            decimal rate = 25_000m;
            int totalOrders = 0;

            while (!token.IsCancellationRequested)
            {
                // Giả lập lấy dữ liệu mới (tỷ giá dao động, quét đơn hàng mới)
                rate = Math.Clamp(Math.Round(rate + (decimal)(Random.Shared.NextDouble() - 0.5) * 120m), 24_000m, 26_000m);
                int newOrders = Random.Shared.Next(0, 6);
                totalOrders += newOrders;

                // KHÔNG được viết lblRate.Text = ... ở đây -> sẽ ném InvalidOperationException (cross-thread)
                PushToUi(new SyncSnapshot(DateTime.Now, rate, newOrders, totalOrders));

                try { await Task.Delay(2000, token); }
                catch (OperationCanceledException) { break; }
            }
        }, token);
    }

    private void StopSync()
    {
        _syncCts?.Cancel();
        _syncCts = null;
        btnToggleSync.Text = "Bật đồng bộ";
    }

    // Được gọi từ THREAD PHỤ: chuyển ngữ cảnh về UI Thread trước khi đụng tới control
    private void PushToUi(SyncSnapshot snapshot)
    {
        try
        {
            if (IsDisposed || !IsHandleCreated) return;

            if (InvokeRequired)                                   // đang ở thread phụ?
                Invoke(new Action(() => ApplySnapshot(snapshot)));  // this.Invoke: chạy hàm trên UI Thread
            else
                ApplySnapshot(snapshot);
        }
        catch (ObjectDisposedException) { }       // form vừa đóng trong lúc đang đồng bộ
        catch (InvalidOperationException) { }     // handle đã bị hủy khi đóng form
    }

    // Chỉ chạy ở UI Thread: được phép gán Label, StatusStrip, thêm dòng vào DataGridView
    private void ApplySnapshot(SyncSnapshot s)
    {
        lblRate.Text = $"Tỷ giá USD/VND: {s.Rate.ToString("N0", _vi)}";
        lblNewOrders.Text = $"Đơn hàng mới: +{s.NewOrders} (tổng {s.TotalOrders})";
        lblLastSync.Text = $"Cập nhật lúc: {s.Time:HH:mm:ss}";

        _syncLog.Insert(0, new SyncLogRow
        {
            Time = s.Time.ToString("HH:mm:ss"),
            Rate = s.Rate,
            NewOrders = s.NewOrders,
            TotalOrders = s.TotalOrders
        });
        while (_syncLog.Count > 100) _syncLog.RemoveAt(_syncLog.Count - 1);

        // Cập nhật StatusStrip của Form cha
        if (MdiParent is frmMain { IsDisposed: false } main)
            main.SetSyncText($"Đồng bộ {s.Time:HH:mm:ss}  |  USD/VND: {s.Rate.ToString("N0", _vi)}  |  Đơn mới: +{s.NewOrders}");
    }

    // =====================================================================
    // DỌN DẸP KHI ĐÓNG FORM
    // =====================================================================
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _syncCts?.Cancel();
        _reportCts?.Cancel();
        base.OnFormClosed(e);
    }
}
