namespace Bai3QuanLySieuThi;

// Đề chỉ yêu cầu 2 form con (Sản phẩm, Báo cáo), nhưng menu có mục "Quản lý Nhập xuất".
// Đây là form khung để menu hoạt động và để kiểm thử cơ chế "mở 1 lần, mở lại thì Activate".
// Bạn có thể phát triển thêm (phiếu nhập/xuất) nếu muốn.
public class frmStockMovement : Form
{
    public frmStockMovement()
    {
        Text = "Quản lý Nhập xuất";
        ClientSize = new Size(640, 400);

        Controls.Add(new Label
        {
            Text = "Chức năng Quản lý Nhập xuất",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 12f)
        });
    }
}
